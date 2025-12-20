using System.Text;
using System.Text.Json;
using EngAce.Api.Services.AI;

namespace EngAce.Api.Services.AI;

/// <summary>
/// 🤖 Gemini AI Service - Tích hợp Google Gemini API để tạo bài tập TOEIC tự động
/// </summary>
public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;
    private readonly string _apiKey;
    private readonly string _openAiApiKey;
    private readonly string _baseUrl;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
        _openAiApiKey = configuration["OpenAI:ApiKey"] ?? string.Empty;
        _baseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
        
        // Validate and log API key status
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogError("❌ Gemini API key is NOT configured. Please set Gemini:ApiKey in appsettings.json or appsettings.Development.json");
        }
        else
        {
            // Mask API key for logging (show first 10 and last 4 chars only)
            var maskedKey = _apiKey.Length > 14 
                ? $"{_apiKey.Substring(0, 10)}...{_apiKey.Substring(_apiKey.Length - 4)}" 
                : "***masked***";
            _logger.LogInformation("✓ Gemini API key configured (length: {Length}, masked: {Masked})", _apiKey.Length, maskedKey);
            _logger.LogInformation("✓ Using model: gemini-2.5-flash");
            _logger.LogInformation("✓ API endpoint: {Endpoint}", _baseUrl);
        }
        
        // Validate and log OpenAI API key status
        if (string.IsNullOrWhiteSpace(_openAiApiKey))
        {
            _logger.LogWarning("⚠️ OpenAI API key is NOT configured. OpenAI provider will not be available.");
        }
        else
        {
            // Mask OpenAI API key for logging (show first 7 and last 4 chars only)
            var maskedOpenAIKey = _openAiApiKey.Length > 11 
                ? $"{_openAiApiKey.Substring(0, 7)}...{_openAiApiKey.Substring(_openAiApiKey.Length - 4)}" 
                : "***masked***";
            _logger.LogInformation("✓ OpenAI API key configured (length: {Length}, masked: {Masked})", _openAiApiKey.Length, maskedOpenAIKey);
        }
    }

    // 🤖 Gọi Gemini API với retry logic và error handling
    public async Task<string> GenerateResponse(string prompt, int maxOutputTokens = 4096)
    {
        try
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = maxOutputTokens,
                    stopSequences = new string[0]
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            int retryCount = 0;
            int maxRetries = 3;
            int baseDelayMs = 1000;

            while (retryCount <= maxRetries)
            {
                try
                {
                    // Build full URL with API key as query parameter
                    var url = $"{_baseUrl}?key={Uri.EscapeDataString(_apiKey)}";
                    _logger.LogDebug("Calling Gemini API: {Url} (API key masked)", _baseUrl);
                    response = await _httpClient.PostAsync(url, content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        break; // Success, exit retry loop
                    }
                    
                    // Read error response body for better error messages
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Gemini API returned {StatusCode}. Response: {ErrorBody}", response.StatusCode, errorBody);
                    
                    // Don't retry for authentication/authorization errors (401, 403) - these are permanent errors
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                        response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        var errorMessage = $"Gemini API authentication failed (Status: {response.StatusCode}). " +
                                         $"Please check your API key in appsettings.json. " +
                                         $"API Key is configured: {!string.IsNullOrEmpty(_apiKey)}. " +
                                         $"Error details: {errorBody}";
                        _logger.LogError(errorMessage);
                        throw new HttpRequestException(errorMessage);
                    }
                    
                    // Don't retry for other client errors (4xx) except 429 (Too Many Requests)
                    if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500 && 
                        response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
                    {
                        var errorMessage = $"Gemini API client error (Status: {response.StatusCode}). Error details: {errorBody}";
                        _logger.LogError(errorMessage);
                        throw new HttpRequestException(errorMessage);
                    }
                    
                    // If it's a 503 or 429, retry with exponential backoff
                    if ((response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable || 
                         response.StatusCode == System.Net.HttpStatusCode.TooManyRequests) && 
                        retryCount < maxRetries)
                    {
                        int delayMs = baseDelayMs * (int)Math.Pow(2, retryCount);
                        _logger.LogWarning("Gemini API returned {StatusCode}. Retrying in {DelayMs}ms... (Attempt {RetryCount}/{MaxRetries})", 
                                         response.StatusCode, delayMs, retryCount + 1, maxRetries);
                        await Task.Delay(delayMs);
                        retryCount++;
                        continue;
                    }
                    
                    // For other errors, throw with error details
                    throw new HttpRequestException($"Gemini API returned {response.StatusCode}. Error details: {errorBody}");
                }
                catch (HttpRequestException ex)
                {
                    // Check if it's a 401/403 error - don't retry
                    if (ex.Message.Contains("401") || ex.Message.Contains("403") || 
                        ex.Message.Contains("authentication failed") || 
                        ex.Message.Contains("Forbidden") || 
                        ex.Message.Contains("Unauthorized"))
                    {
                        throw; // Re-throw authentication errors immediately
                    }
                    
                    // Check if it's a 4xx client error - don't retry
                    if (ex.Message.Contains("400") || ex.Message.Contains("404") || 
                        ex.Message.Contains("client error"))
                    {
                        throw; // Re-throw client errors immediately
                    }
                    
                    // Retry for network errors or 5xx errors
                    if (retryCount < maxRetries)
                    {
                        int delayMs = baseDelayMs * (int)Math.Pow(2, retryCount);
                        _logger.LogWarning("Network error calling Gemini API: {Error}. Retrying in {DelayMs}ms... (Attempt {RetryCount}/{MaxRetries})", 
                                         ex.Message, delayMs, retryCount + 1, maxRetries);
                        await Task.Delay(delayMs);
                        retryCount++;
                        continue;
                    }
                    
                    // Max retries reached
                    throw;
                }
            }
            
            // Final check
            if (response != null && !response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                var errorMessage = $"Gemini API failed after {maxRetries} retries. Final status: {response.StatusCode}. Error details: {errorBody}";
                _logger.LogError(errorMessage);
                throw new HttpRequestException(errorMessage);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            
            // Log the full response for debugging
            _logger.LogInformation("Full Gemini response: {Response}", responseContent);

            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent);

            if (geminiResponse?.Candidates?.Count > 0 && 
                geminiResponse.Candidates[0].Content?.Parts?.Count > 0)
            {
                var text = geminiResponse.Candidates[0].Content.Parts[0].Text;
                _logger.LogInformation("Extracted text from Gemini: {Text}", text?.Substring(0, Math.Min(text.Length, 200)) + "...");
                return text;
            }

            _logger.LogWarning("No valid response from Gemini API. Candidates count: {Count}, First candidate content: {Content}",
                geminiResponse?.Candidates?.Count ?? 0,
                geminiResponse?.Candidates?.FirstOrDefault()?.Content?.ToString() ?? "null");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            throw;
        }
    }

    // 🤖 Gọi OpenAI API với retry logic và error handling
    public async Task<string> GenerateResponseOpenAI(string prompt, int maxTokens = 4096)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_openAiApiKey))
            {
                throw new InvalidOperationException("OpenAI API key is not configured");
            }

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = maxTokens
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");

            HttpResponseMessage response = null;
            int retryCount = 0;
            int maxRetries = 3;
            int baseDelayMs = 1000;

            while (retryCount <= maxRetries)
            {
                try
                {
                    var url = "https://api.openai.com/v1/chat/completions";
                    _logger.LogDebug("Calling OpenAI API: {Url} (API key masked)", url);
                    response = await _httpClient.PostAsync(url, content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        break;
                    }
                    
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("OpenAI API returned {StatusCode}. Response: {ErrorBody}", response.StatusCode, errorBody);
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                        response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        var errorMessage = $"OpenAI API authentication failed (Status: {response.StatusCode}). Error details: {errorBody}";
                        _logger.LogError(errorMessage);
                        throw new HttpRequestException(errorMessage);
                    }
                    
                    if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500 && 
                        response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
                    {
                        var errorMessage = $"OpenAI API client error (Status: {response.StatusCode}). Error details: {errorBody}";
                        _logger.LogError(errorMessage);
                        throw new HttpRequestException(errorMessage);
                    }
                    
                    if ((response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable || 
                         response.StatusCode == System.Net.HttpStatusCode.TooManyRequests) && 
                        retryCount < maxRetries)
                    {
                        int delayMs = baseDelayMs * (int)Math.Pow(2, retryCount);
                        _logger.LogWarning("OpenAI API returned {StatusCode}. Retrying in {DelayMs}ms... (Attempt {RetryCount}/{MaxRetries})", 
                                         response.StatusCode, delayMs, retryCount + 1, maxRetries);
                        await Task.Delay(delayMs);
                        retryCount++;
                        continue;
                    }
                    
                    throw new HttpRequestException($"OpenAI API returned {response.StatusCode}. Error details: {errorBody}");
                }
                catch (HttpRequestException ex)
                {
                    if (ex.Message.Contains("401") || ex.Message.Contains("403") || 
                        ex.Message.Contains("authentication failed") || 
                        ex.Message.Contains("Forbidden") || 
                        ex.Message.Contains("Unauthorized"))
                    {
                        throw;
                    }
                    
                    if (ex.Message.Contains("400") || ex.Message.Contains("404") || 
                        ex.Message.Contains("client error"))
                    {
                        throw;
                    }
                    
                    if (retryCount < maxRetries)
                    {
                        int delayMs = baseDelayMs * (int)Math.Pow(2, retryCount);
                        _logger.LogWarning("Network error calling OpenAI API: {Error}. Retrying in {DelayMs}ms... (Attempt {RetryCount}/{MaxRetries})", 
                                         ex.Message, delayMs, retryCount + 1, maxRetries);
                        await Task.Delay(delayMs);
                        retryCount++;
                        continue;
                    }
                    
                    throw;
                }
            }
            
            if (response != null && !response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                var errorMessage = $"OpenAI API failed after {maxRetries} retries. Final status: {response.StatusCode}. Error details: {errorBody}";
                _logger.LogError(errorMessage);
                throw new HttpRequestException(errorMessage);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Full OpenAI response: {Response}", responseContent);

            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;
            
            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var message = choices[0].GetProperty("message").GetProperty("content").GetString();
                _logger.LogInformation("Extracted text from OpenAI: {Text}", message?.Substring(0, Math.Min(message?.Length ?? 0, 200)) + "...");
                return message ?? string.Empty;
            }

            _logger.LogWarning("No valid response from OpenAI API");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            throw;
        }
    }

    // 🤖 Tạo câu hỏi từ AI (Part 5) - Build prompt -> Call API -> Parse response
    public async Task<List<GeneratedQuestion>> GenerateQuestionsAsync(string content, string exerciseType, string level, int questionCount = 5, string provider = "gemini")
    {
        try
        {
            string prompt = BuildPrompt(content, exerciseType, level, questionCount);
            _logger.LogInformation("Sending prompt to {Provider} for {ExerciseType} with {QuestionCount} questions", provider, exerciseType, questionCount);
            
            string response = provider.ToLower() == "openai" 
                ? await GenerateResponseOpenAI(prompt)
                : await GenerateResponse(prompt);
            
            _logger.LogInformation("Received response from {Provider} with length: {Length}", provider, response.Length);
            
            if (string.IsNullOrWhiteSpace(response))
            {
                _logger.LogWarning("Empty response from Gemini API");
                return new List<GeneratedQuestion>();
            }

            return ParseGeneratedQuestions(response);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("403") || ex.Message.Contains("401") || ex.Message.Contains("authentication failed"))
        {
            _logger.LogError(ex, "Gemini API authentication failed for type: {ExerciseType}. Cannot generate questions.", exerciseType);
            // Return empty questions list so controller can use fallback
            return new List<GeneratedQuestion>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating questions for type: {ExerciseType}", exerciseType);
            
            // Check if it's a service unavailable error and provide fallback
            if (ex is HttpRequestException httpEx && httpEx.Message.Contains("503"))
            {
                _logger.LogWarning("Gemini API is temporarily unavailable (503). Returning sample fallback response for {ExerciseType}", exerciseType);
                return GetFallbackQuestions(exerciseType, level, questionCount);
            }
            
            // For other errors, return empty so controller can use fallback
            _logger.LogWarning("AI generation failed, will use fallback questions");
            return new List<GeneratedQuestion>();
        }
    }

    // 🤖 Tạo câu hỏi + passage từ AI (Part 6/7) - Build prompt -> Call API -> Parse questions & passage
    public async Task<(List<GeneratedQuestion> questions, string passage)> GenerateQuestionsWithPassageAsync(string content, string exerciseType, string level, int questionCount = 5, string provider = "gemini")
    {
        try
        {
            string prompt = BuildPrompt(content, exerciseType, level, questionCount);
            _logger.LogInformation("Sending prompt to {Provider} for {ExerciseType} with {QuestionCount} questions (with passage)", provider, exerciseType, questionCount);
            
            // Use higher maxTokens for Part 6/7 to avoid truncated responses
            int maxTokens = 8192;
            string response = provider.ToLower() == "openai" 
                ? await GenerateResponseOpenAI(prompt, maxTokens)
                : await GenerateResponse(prompt, maxTokens);
            
            _logger.LogInformation("Received response from {Provider} with length: {Length}", provider, response.Length);
            
            if (string.IsNullOrWhiteSpace(response))
            {
                _logger.LogWarning("Empty response from Gemini API");
                return (new List<GeneratedQuestion>(), content);
            }

            // Parse the response to extract both questions and passage
            var questions = new List<GeneratedQuestion>();
            var passage = content; // fallback to original content
            
            try
            {
                var cleanedResponse = CleanJsonResponse(response);
                _logger.LogInformation("Attempting to parse Part7Response for questions and passage");
                
                // Try to parse complete Part7Response
                try
                {
                    var part7Response = JsonSerializer.Deserialize<Part7Response>(cleanedResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (part7Response != null)
                    {
                        if (part7Response.Questions?.Any() == true)
                        {
                            questions = part7Response.Questions;
                            _logger.LogInformation("Successfully extracted {Count} questions", questions.Count);
                        }
                        
                        if (!string.IsNullOrEmpty(part7Response.Passage))
                        {
                            passage = part7Response.Passage;
                            _logger.LogInformation("Successfully extracted passage with length: {Length}", passage.Length);
                        }
                    }
                }
                catch (JsonException jsonEx)
                {
                    // If JSON is incomplete (truncated), try to extract partial data
                    _logger.LogWarning(jsonEx, "JSON parsing failed (possibly truncated), attempting partial extraction");
                    
                    // Try to extract passage and questions separately from incomplete JSON
                    var extracted = ExtractPartialDataFromIncompleteJson(cleanedResponse);
                    if (extracted.passage != null)
                    {
                        passage = extracted.passage;
                        _logger.LogInformation("Extracted passage from incomplete JSON, length: {Length}", passage.Length);
                    }
                    if (extracted.questions.Any())
                    {
                        questions = extracted.questions;
                        _logger.LogInformation("Extracted {Count} questions from incomplete JSON", questions.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse Part7Response, falling back to question-only parsing");
                questions = ParseGeneratedQuestions(response);
                
                // Try to extract passage even if questions parsing failed
                var passageMatch = System.Text.RegularExpressions.Regex.Match(response, @"""passage""\s*:\s*""([^""]+)");
                if (passageMatch.Success)
                {
                    passage = passageMatch.Groups[1].Value;
                    _logger.LogInformation("Extracted passage using regex fallback, length: {Length}", passage.Length);
                }
            }
            
            // Ensure minimum questions for Part 6 (4 questions) and Part 7 (5 questions)
            var minQuestions = exerciseType?.ToLower() == "part 6" || exerciseType?.ToLower() == "part6" ? 4 : questionCount;
            if (questions.Count < minQuestions)
            {
                var fallbackQuestions = GetFallbackQuestions(exerciseType, level, minQuestions);
                if (questions.Any())
                {
                    // Merge parsed questions with fallback to reach minimum
                    while (questions.Count < minQuestions && fallbackQuestions.Count > 0)
                    {
                        var fallbackQ = fallbackQuestions.FirstOrDefault(q => 
                            !questions.Any(existing => existing.QuestionText == q.QuestionText));
                        if (fallbackQ != null)
                        {
                            questions.Add(fallbackQ);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    // No questions parsed, use fallback
                    questions = fallbackQuestions;
                    _logger.LogWarning("No questions parsed, using fallback questions");
                }
            }
            
            // Ensure passage is not empty (fallback to base content if needed)
            if (string.IsNullOrWhiteSpace(passage) || passage.Length < 50)
            {
                passage = content;
                _logger.LogWarning("Passage too short or empty, using base content");
            }
            
            return (questions, passage);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("403") || ex.Message.Contains("401") || ex.Message.Contains("authentication failed"))
        {
            _logger.LogError(ex, "Gemini API authentication failed for type: {ExerciseType}. Cannot generate questions.", exerciseType);
            // Return empty questions list so controller can use fallback
            return (new List<GeneratedQuestion>(), content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating questions with passage for type: {ExerciseType}", exerciseType);
            
            // Check if it's a service unavailable error and provide fallback
            if (ex is HttpRequestException httpEx && httpEx.Message.Contains("503"))
            {
                _logger.LogWarning("Gemini API is temporarily unavailable (503). Returning sample fallback response for {ExerciseType}", exerciseType);
                return (GetFallbackQuestions(exerciseType, level, questionCount), content);
            }
            
            // For other errors, return empty so controller can use fallback
            _logger.LogWarning("AI generation failed, will use fallback questions");
            return (new List<GeneratedQuestion>(), content);
        }
    }

    public async Task<string> GenerateExplanationAsync(string questionText, string correctAnswer)
    {
        try
        {
            string prompt = $"Explain why '{correctAnswer}' is the correct answer for this question: {questionText}";
            return await GenerateResponse(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating explanation");
            throw;
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            string testResponse = await GenerateResponse("Test connection");
            return !string.IsNullOrWhiteSpace(testResponse);
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetRawGeminiResponseAsync(string content, string exerciseType, string level, int questionCount)
    {
        try
        {
            string prompt = BuildPrompt(content, exerciseType, level, questionCount);
            return await GenerateResponse(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting raw Gemini response");
            return null;
        }
    }

    private string BuildPrompt(string content, string exerciseType, string level, int questionCount)
    {
        var basePrompt = $@"You are an expert English language teacher creating TOEIC exercises.

TOPIC: {content}
EXERCISE LEVEL: {level}
EXERCISE TYPE: {exerciseType}
NUMBER OF QUESTIONS: {questionCount}

CRITICAL REQUIREMENTS:
1. Return ONLY valid JSON - no markdown, no explanations, no additional text
2. Do not include ```json or ``` code blocks
3. Follow the exact JSON format specified below
4. Ensure all JSON is properly escaped

";

        switch (exerciseType?.ToLower())
        {
            case "part 5":
            case "part5":
                return basePrompt + BuildPart5Prompt(level, questionCount);
            
            case "part 6":
            case "part6":
                return basePrompt + BuildPart6Prompt(level, questionCount);
            
            case "part 7":
            case "part7":
                return basePrompt + BuildPart7Prompt(level, questionCount);
                
            default:
                return basePrompt + BuildDefaultPrompt(level, questionCount);
        }
    }

    private string BuildPart6Prompt(string level, int questionCount)
    {
        var wordLimit = GetWordLimit(level, "part6");
        
        return $@"Create a Part 6 Text Completion exercise with the following specifications:

WORD LIMIT: Maximum {wordLimit} words for the passage.

Format Requirements:
- Create a passage with exactly 4 blanks (marked as _____)
- Questions should increase in difficulty:
  1. Basic grammar/verb forms (e.g., past tense, present perfect)
  2. Vocabulary in context (e.g., appropriate word choice)
  3. Transition words/connectors (e.g., however, therefore, moreover)
  4. Advanced grammar/complex structures (e.g., conditionals, subjunctive)

Difficulty by Level:
- {GetDifficultyGuidelines(level)}

Return in this exact JSON format:
{{
  ""passage"": ""Your passage text with _____ for blanks"",
  ""questions"": [
    {{
      ""questionText"": ""Question 1"",
      ""options"": [""A"", ""B"", ""C"", ""D""],
      ""correctAnswer"": 1,
      ""explanation"": ""Why this answer is correct"",
      ""difficulty"": 1
    }}
  ]
}}";
    }

    private string BuildPart5Prompt(string level, int questionCount)
    {
        return $@"Create a Part 5 Incomplete Sentences (Grammar) exercise with the following specifications:

Question Count: {questionCount} questions (typically 5 questions)
Difficulty Level: {level}

Format Requirements:
- Each question tests grammar, vocabulary, or sentence structure
- Questions should increase in difficulty from 1 to {questionCount}
- Focus on TOEIC business/workplace contexts

Grammar Focus by Question Number:
1. Basic verb forms/tenses (present, past, future)
2. Articles, prepositions, basic word forms
3. Comparative/superlative, conditional forms
4. Passive voice, gerunds/infinitives
5. Complex structures, subjunctive, advanced syntax

Difficulty Guidelines:
- {GetDifficultyGuidelines(level)}

Business Context Examples:
- Office communications, meetings, reports
- Company policies, procedures, announcements  
- Business travel, conferences, presentations
- Sales, marketing, customer service
- Finance, accounting, project management

Return in this exact JSON format (array of questions only):
[
  {{
    ""questionText"": ""The meeting _____ scheduled for 3 PM tomorrow."",
    ""options"": [""is"", ""are"", ""was"", ""were""],
    ""correctAnswer"": 1,
    ""explanation"": ""'Is' is correct because 'meeting' is singular and the sentence refers to future time"",
    ""difficulty"": 1
  }}
]";
    }

    private string BuildPart7Prompt(string level, int questionCount)
    {
        var wordLimit = GetWordLimit(level, "part7");
        var passageCount = GetPassageCount(level);
        
        // Thêm instructions cụ thể cho 2 đoạn văn ở Advanced level
        var passageInstruction = passageCount == 2 
            ? "Create TWO related passages (e.g., an email and a follow-up memo, or an article and a response letter). The passages should be connected but provide different perspectives or additional information."
            : "Create ONE comprehensive passage";
        
        return $@"Create a Part 7 Reading Comprehension exercise with the following specifications:

WORD LIMIT: Maximum {wordLimit} words total for all passages.
PASSAGE COUNT: {passageCount} passage(s)
PASSAGE INSTRUCTION: {passageInstruction}

Question Count: {questionCount} questions

Question Types:
- Main idea/purpose
- Detail/specific information
- Inference/implied meaning
- Vocabulary in context
- Author's tone/attitude

Difficulty Guidelines:
- {GetDifficultyGuidelines(level)}

CRITICAL: You must return ONLY valid JSON in the exact format below. No markdown, no explanations, no additional text.

{{
  ""passage"": ""{(passageCount == 2 ? "First passage content\n\nSecond passage content" : "A complete reading passage about the topic")} (within {wordLimit} words total)"",
  ""questions"": [
    {{
      ""questionText"": ""What is the main purpose of this passage?"",
      ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
      ""correctAnswer"": 0,
      ""explanation"": ""Detailed explanation of why this answer is correct"",
      ""difficulty"": {(level?.ToLower() == "beginner" ? 1 : level?.ToLower() == "intermediate" ? 2 : 3)}
    }}
  ]
}}";
    }

    private string BuildDefaultPrompt(string level, int questionCount)
    {
        return $@"Create {questionCount} multiple-choice questions based on the content.

Difficulty: {level}

Return in this exact JSON format:
{{
  ""questions"": [
    {{
      ""questionText"": ""Your question here"",
      ""options"": [""A"", ""B"", ""C"", ""D""],
      ""correctAnswer"": 1,
      ""explanation"": ""Why this answer is correct"",
      ""difficulty"": 1
    }}
  ]
}}";
    }

    private int GetWordLimit(string level, string exerciseType)
    {
        if (exerciseType?.ToLower() == "part6")
        {
            return level?.ToLower() switch
            {
                "beginner" => 120,
                "intermediate" => 150,
                "advanced" => 180,
                _ => 150
            };
        }
        else if (exerciseType?.ToLower() == "part7")
        {
            return level?.ToLower() switch
            {
                "beginner" => 150,
                "intermediate" => 200,
                "advanced" => 250,
                _ => 200
            };
        }
        
        return 200;
    }

    private int GetPassageCount(string level)
    {
        return level?.ToLower() switch
        {
            "beginner" => 1,
            "intermediate" => 1,
            "advanced" => 2,
            _ => 1
        };
    }

    private string GetDifficultyGuidelines(string level)
    {
        return level?.ToLower() switch
        {
            "beginner" => "Use simple vocabulary and sentence structures, focus on basic grammar",
            "intermediate" => "Use moderate vocabulary, include some complex sentences and intermediate grammar",
            "advanced" => "Use sophisticated vocabulary, complex sentence structures, and advanced grammar concepts",
            _ => "Use appropriate vocabulary and grammar for the target audience"
        };
    }

    private List<GeneratedQuestion> ParseGeneratedQuestions(string generatedText)
    {
        var questions = new List<GeneratedQuestion>();
        
        if (string.IsNullOrWhiteSpace(generatedText))
        {
            _logger.LogWarning("Empty response from Gemini");
            return questions;
        }

        try
        {
            // Clean up the response (remove markdown code blocks if any)
            var cleanedResponse = CleanJsonResponse(generatedText);
            
            // Try to parse as Part7Response first (for Part 6 and Part 7)
            try
            {
                _logger.LogInformation("Attempting to parse as Part7Response. Response length: {Length}", cleanedResponse.Length);
                _logger.LogInformation("Response preview: {Preview}...", cleanedResponse.Substring(0, Math.Min(cleanedResponse.Length, 500)));
                
                var part7Response = JsonSerializer.Deserialize<Part7Response>(cleanedResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (part7Response?.Questions?.Any() == true)
                {
                    _logger.LogInformation("Successfully parsed as Part7Response with {Count} questions", part7Response.Questions.Count);
                    return part7Response.Questions;
                }
                else
                {
                    _logger.LogWarning("Part7Response parsed but has no questions. Passage: {HasPassage}, Questions count: {Count}", 
                        !string.IsNullOrEmpty(part7Response?.Passage), part7Response?.Questions?.Count ?? 0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse as Part7Response, trying fallback parsing");
            }

            // Fallback 1: Try to parse as simple questions array (Part 5)
            try
            {
                using var document = JsonDocument.Parse(cleanedResponse);
                var root = document.RootElement;
                
                // Check if root is directly an array (Part 5 format)
                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var questionElement in root.EnumerateArray())
                    {
                        var question = ParseSingleQuestion(questionElement);
                        if (question != null)
                        {
                            questions.Add(question);
                        }
                    }
                    _logger.LogInformation("Parsed {Count} questions as direct array (Part 5 format)", questions.Count);
                }
                // Check for questions property in object (fallback format)
                else if (root.TryGetProperty("questions", out var questionsElement) && questionsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var questionElement in questionsElement.EnumerateArray())
                    {
                        var question = ParseSingleQuestion(questionElement);
                        if (question != null)
                        {
                            questions.Add(question);
                        }
                    }
                    _logger.LogInformation("Parsed {Count} questions using fallback object format", questions.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse JSON response: {Response}", cleanedResponse);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing generated questions");
        }

        // Final fallback: create a basic question if we couldn't parse anything
        if (!questions.Any())
        {
            _logger.LogWarning("No questions parsed, creating fallback question");
            questions.Add(new GeneratedQuestion
            {
                QuestionText = "Generated question based on the content",
                Options = new List<string> { "Option A", "Option B", "Option C", "Option D" },
                CorrectAnswer = 0,
                Explanation = "This question was generated as a fallback",
                Difficulty = 3
            });
        }

        return questions;
    }

    private string CleanJsonResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return response;

        // Remove markdown code blocks
        var cleaned = response.Trim();
        if (cleaned.StartsWith("```json"))
        {
            cleaned = cleaned.Substring(7);
        }
        if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned.Substring(3);
        }
        if (cleaned.EndsWith("```"))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 3);
        }
        
        return cleaned.Trim();
    }

    /// <summary>
    /// Extract partial data from incomplete/truncated JSON response
    /// Tries to extract passage and questions even if JSON is incomplete
    /// </summary>
    private (string? passage, List<GeneratedQuestion> questions) ExtractPartialDataFromIncompleteJson(string json)
    {
        var passage = (string?)null;
        var questions = new List<GeneratedQuestion>();

        try
        {
            // Try to extract passage using regex (handle escaped quotes)
            var passagePattern = @"""passage""\s*:\s*""((?:[^""\\]|\\.|\\n)*)";
            var passageMatch = System.Text.RegularExpressions.Regex.Match(json, passagePattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            
            if (passageMatch.Success && passageMatch.Groups.Count > 1)
            {
                passage = passageMatch.Groups[1].Value
                    .Replace("\\n", "\n")
                    .Replace("\\\"", "\"")
                    .Replace("\\\\", "\\")
                    .Replace("\\r", "\r")
                    .Replace("\\t", "\t");
            }

            // Try to extract questions by finding complete question objects
            // Look for pattern: {"questionText": "...", "options": [...], "correctAnswer": ...}
            var questionPattern = @"\{[^{}]*""questionText""\s*:\s*""[^""]+"".*?""options""\s*:\s*\[[^\]]+\].*?""correctAnswer""\s*:\s*\d+";
            var questionMatches = System.Text.RegularExpressions.Regex.Matches(json, questionPattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Singleline);
            
            foreach (System.Text.RegularExpressions.Match match in questionMatches)
            {
                try
                {
                    // Try to close the JSON object if incomplete
                    var questionJson = match.Value;
                    if (!questionJson.TrimEnd().EndsWith("}"))
                    {
                        questionJson += "}";
                    }
                    
                    using var qDoc = JsonDocument.Parse(questionJson);
                    var q = ParseSingleQuestion(qDoc.RootElement);
                    if (q != null)
                    {
                        questions.Add(q);
                    }
                }
                catch
                {
                    // Skip incomplete question
                }
            }

            // If regex extraction didn't work, try to manually parse from questions array
            if (!questions.Any())
            {
                var questionsStart = json.IndexOf("\"questions\"", StringComparison.OrdinalIgnoreCase);
                if (questionsStart > 0)
                {
                    var questionsArrayStart = json.IndexOf("[", questionsStart);
                    if (questionsArrayStart > 0)
                    {
                        var bracketCount = 0;
                        var inString = false;
                        var escapeNext = false;
                        var endPos = questionsArrayStart;
                        
                        for (var i = questionsArrayStart; i < json.Length && i < questionsArrayStart + 5000; i++)
                        {
                            var ch = json[i];
                            
                            if (escapeNext)
                            {
                                escapeNext = false;
                                continue;
                            }
                            
                            if (ch == '\\')
                            {
                                escapeNext = true;
                                continue;
                            }
                            
                            if (ch == '"' && !escapeNext)
                            {
                                inString = !inString;
                                continue;
                            }
                            
                            if (!inString)
                            {
                                if (ch == '[')
                                {
                                    bracketCount++;
                                }
                                else if (ch == ']')
                                {
                                    bracketCount--;
                                    if (bracketCount == 0)
                                    {
                                        endPos = i + 1;
                                        break;
                                    }
                                }
                            }
                        }
                        
                        if (endPos > questionsArrayStart)
                        {
                            var questionsJson = json.Substring(questionsArrayStart, endPos - questionsArrayStart);
                            try
                            {
                                using var document = JsonDocument.Parse(questionsJson);
                                var root = document.RootElement;
                                
                                if (root.ValueKind == JsonValueKind.Array)
                                {
                                    foreach (var questionElement in root.EnumerateArray())
                                    {
                                        var q = ParseSingleQuestion(questionElement);
                                        if (q != null)
                                        {
                                            questions.Add(q);
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // Skip if parsing fails
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting partial data from incomplete JSON");
        }

        return (passage, questions);
    }

    private GeneratedQuestion? ParseSingleQuestion(JsonElement questionElement)
    {
        try
        {
            var questionText = questionElement.TryGetProperty("questionText", out var qtProp) ? qtProp.GetString() : null;
            var correctAnswer = questionElement.TryGetProperty("correctAnswer", out var caProp) ? caProp.GetInt32() : 0;
            var explanation = questionElement.TryGetProperty("explanation", out var expProp) ? expProp.GetString() : "";
            var difficulty = questionElement.TryGetProperty("difficulty", out var diffProp) ? diffProp.GetInt32() : 1;

            var options = new List<string>();
            if (questionElement.TryGetProperty("options", out var optionsProp) && optionsProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var option in optionsProp.EnumerateArray())
                {
                    if (option.ValueKind == JsonValueKind.String)
                    {
                        options.Add(option.GetString() ?? "");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(questionText) && options.Count >= 2)
            {
                return new GeneratedQuestion
                {
                    QuestionText = questionText,
                    Options = options,
                    CorrectAnswer = correctAnswer,
                    Explanation = explanation ?? "",
                    Difficulty = difficulty
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse single question element");
        }

        return null;
    }

    private List<GeneratedQuestion> GetFallbackQuestions(string exerciseType, string level, int questionCount)
    {
        _logger.LogInformation("Generating fallback questions for {ExerciseType}, level {Level}", exerciseType, level);
        
        var questions = new List<GeneratedQuestion>();
        
        if (exerciseType == "Part 7")
        {
            // Advanced level fallback for Part 7 with 2 passages
            if (level == "Advanced")
            {
                questions.AddRange(new List<GeneratedQuestion>
                {
                    new GeneratedQuestion
                    {
                        QuestionText = "According to the email, what is the main purpose of the meeting?",
                        Options = new List<string> 
                        { 
                            "To discuss quarterly sales results",
                            "To plan the annual company retreat", 
                            "To introduce new team members",
                            "To review budget allocations"
                        },
                        CorrectAnswer = 0,
                        Explanation = "The email explicitly states that the meeting is to discuss quarterly sales results.",
                        Difficulty = 3
                    },
                    new GeneratedQuestion
                    {
                        QuestionText = "What action does the follow-up memo recommend?",
                        Options = new List<string> 
                        { 
                            "Increase marketing budget",
                            "Hire additional staff", 
                            "Implement new sales strategies",
                            "Reduce operational costs"
                        },
                        CorrectAnswer = 2,
                        Explanation = "The memo suggests implementing new sales strategies based on the meeting discussion.",
                        Difficulty = 3
                    },
                    new GeneratedQuestion
                    {
                        QuestionText = "When is the next review meeting scheduled?",
                        Options = new List<string> 
                        { 
                            "Next week",
                            "Next month", 
                            "Next quarter",
                            "Next year"
                        },
                        CorrectAnswer = 1,
                        Explanation = "The memo indicates the next review will be held next month.",
                        Difficulty = 2
                    },
                    new GeneratedQuestion
                    {
                        QuestionText = "Who should prepare the sales report mentioned in both documents?",
                        Options = new List<string> 
                        { 
                            "The marketing team",
                            "The sales manager", 
                            "The finance department",
                            "External consultants"
                        },
                        CorrectAnswer = 1,
                        Explanation = "Both the email and memo indicate the sales manager is responsible for the report.",
                        Difficulty = 3
                    }
                });
            }
            else
            {
                // Single passage fallback for other levels
                questions.AddRange(new List<GeneratedQuestion>
                {
                    new GeneratedQuestion
                    {
                        QuestionText = "What is the main topic of the passage?",
                        Options = new List<string> 
                        { 
                            "Home renovation tips",
                            "Interior design trends", 
                            "Real estate investment",
                            "Property maintenance"
                        },
                        CorrectAnswer = 0,
                        Explanation = "The passage primarily discusses various home renovation tips and techniques.",
                        Difficulty = level == "Beginner" ? 1 : 2
                    },
                    new GeneratedQuestion
                    {
                        QuestionText = "According to the text, what should you do before starting a renovation project?",
                        Options = new List<string> 
                        { 
                            "Buy all materials",
                            "Hire contractors", 
                            "Create a detailed plan",
                            "Apply for permits"
                        },
                        CorrectAnswer = 2,
                        Explanation = "The text emphasizes the importance of creating a detailed plan before beginning any renovation work.",
                        Difficulty = level == "Beginner" ? 1 : 2
                    }
                });
            }
        }
        else if (exerciseType == "Part 6" || exerciseType == "part6" || exerciseType?.ToLower() == "part 6")
        {
            // Part 6 fallback with text completion - needs exactly 4 questions
            var difficulty = level?.ToLower() switch
            {
                "beginner" => 1,
                "intermediate" => 2,
                "advanced" => 3,
                _ => 2
            };
            
            questions.AddRange(new List<GeneratedQuestion>
            {
                new GeneratedQuestion
                {
                    QuestionText = "Question 1",
                    Options = new List<string> 
                    { 
                        "will launch",
                        "launching", 
                        "has launched",
                        "launched"
                    },
                    CorrectAnswer = 0,
                    Explanation = "Future tense 'will launch' is appropriate for announcing a future event.",
                    Difficulty = difficulty
                },
                new GeneratedQuestion
                {
                    QuestionText = "Question 2",
                    Options = new List<string> 
                    { 
                        "have been",
                        "has been", 
                        "were been",
                        "was been"
                    },
                    CorrectAnswer = 1,
                    Explanation = "The singular subject 'feedback' requires the singular verb form 'has been'.",
                    Difficulty = difficulty + 1
                },
                new GeneratedQuestion
                {
                    QuestionText = "Question 3",
                    Options = new List<string> 
                    { 
                        "However",
                        "For example", 
                        "In addition",
                        "Therefore"
                    },
                    CorrectAnswer = 2,
                    Explanation = "'In addition' is used to add more information or another point.",
                    Difficulty = difficulty + 1
                },
                new GeneratedQuestion
                {
                    QuestionText = "Question 4",
                    Options = new List<string> 
                    { 
                        "am",
                        "was", 
                        "will be",
                        "being"
                    },
                    CorrectAnswer = 2,
                    Explanation = "The conditional 'if' clause with present tense requires future tense in the main clause.",
                    Difficulty = difficulty + 2
                }
            });
        }
        else // Part 5 fallback
        {
            questions.AddRange(new List<GeneratedQuestion>
            {
                new GeneratedQuestion
                {
                    QuestionText = "The company _____ its employees with excellent benefits.",
                    Options = new List<string> 
                    { 
                        "provides",
                        "provide", 
                        "providing",
                        "provided"
                    },
                    CorrectAnswer = 0,
                    Explanation = "The singular subject 'company' requires the singular verb form 'provides'.",
                    Difficulty = level == "Beginner" ? 1 : (level == "Intermediate" ? 2 : 3)
                },
                new GeneratedQuestion
                {
                    QuestionText = "Please submit your application _____ the deadline.",
                    Options = new List<string> 
                    { 
                        "after",
                        "before", 
                        "during",
                        "through"
                    },
                    CorrectAnswer = 1,
                    Explanation = "Logic dictates that applications should be submitted 'before' the deadline.",
                    Difficulty = level == "Beginner" ? 1 : (level == "Intermediate" ? 2 : 3)
                }
            });
        }
        
        // Ensure we have the requested number of questions by repeating or trimming
        if (questions.Count < questionCount)
        {
            while (questions.Count < questionCount && questions.Count > 0)
            {
                questions.Add(questions[questions.Count % questions.Count]);
            }
        }
        else if (questions.Count > questionCount)
        {
            questions = questions.Take(questionCount).ToList();
        }
        
        _logger.LogInformation("Generated {Count} fallback questions for {ExerciseType}", questions.Count, exerciseType);
        return questions;
    }

    // 🤖 NEW: Generate chat response với provider support
    public async Task<string> GenerateChatResponseAsync(string prompt, string provider = "gemini")
    {
        try
        {
            _logger.LogInformation("Generating chat response with {Provider}", provider);
            
            string response = provider.ToLower() == "openai" 
                ? await GenerateResponseOpenAI(prompt, 2048) // Chat responses shorter
                : await GenerateResponse(prompt, 2048);
            
            return response ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chat response with {Provider}", provider);
            throw;
        }
    }

    // 🤖 NEW: Generate dictionary search với provider support  
    public async Task<string> GenerateDictionarySearchAsync(string prompt, string provider = "gemini")
    {
        try
        {
            _logger.LogInformation("Generating dictionary search with {Provider}", provider);
            
            string response = provider.ToLower() == "openai" 
                ? await GenerateResponseOpenAI(prompt, 3072)
                : await GenerateResponse(prompt, 3072);
            
            return response ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dictionary search with {Provider}", provider);
            throw;
        }
    }

    // 🤖 NEW: Generate quiz/exercise với provider support
    public async Task<string> GenerateQuizResponseAsync(string prompt, string provider = "gemini")
    {
        try
        {
            _logger.LogInformation("Generating quiz with {Provider}", provider);
            
            string response = provider.ToLower() == "openai" 
                ? await GenerateResponseOpenAI(prompt, 4096)
                : await GenerateResponse(prompt, 4096);
            
            return response ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quiz with {Provider}", provider);
            throw;
        }
    }

    /// <summary>
    /// 🤖 Generate AI response with provider and maxTokens support
    /// </summary>
    public async Task<string> GenerateResponseAsync(string prompt, string provider = "gemini", int maxTokens = 2048)
    {
        try
        {
            _logger.LogInformation("Generating response with {Provider}, maxTokens: {MaxTokens}", provider, maxTokens);
            
            string response = provider.ToLower() == "openai" 
                ? await GenerateResponseOpenAI(prompt, maxTokens)
                : await GenerateResponse(prompt, maxTokens);
            
            return response ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response with {Provider}", provider);
            throw;
        }
    }
}
