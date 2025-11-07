using System.Text;
using System.Text.Json;
using EngAce.Api.Services.AI;
using EngAce.Api.Services.Interfaces;

namespace EngAce.Api.Services.AI;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = configuration["Gemini:ApiKey"] ?? "AIzaSyBhj4pAZhz05eIiAANufwmTgizO96H4cjc";
        _baseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
    }

    public async Task<List<GeneratedQuestion>> GenerateQuestionsAsync(string content, string exerciseType, string level, int questionCount = 5)
    {
        try
        {
            var prompt = BuildPrompt(content, exerciseType, level, questionCount);
            var response = await CallGeminiApiAsync(prompt);
            
            if (response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text != null)
            {
                var generatedText = response.Candidates.First().Content.Parts.First().Text;
                return ParseGeneratedQuestions(generatedText);
            }

            _logger.LogWarning("No response from Gemini API");
            return new List<GeneratedQuestion>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating questions with Gemini AI: {Message}", ex.Message);
            return new List<GeneratedQuestion>();
        }
    }

    public async Task<string> GenerateExplanationAsync(string questionText, string correctAnswer)
    {
        try
        {
            var prompt = $@"
Explain why this is the correct answer for this English question:

Question: {questionText}
Correct Answer: {correctAnswer}

Provide a clear, concise explanation suitable for English learners.";

            var response = await CallGeminiApiAsync(prompt);
            return response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "No explanation available";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating explanation: {Message}", ex.Message);
            return "Explanation not available";
        }
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var response = await CallGeminiApiAsync("Hello, are you working?");
            return response?.Candidates?.Any() == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini connection test failed: {Message}", ex.Message);
            return false;
        }
    }

    public async Task<string?> GetRawGeminiResponseAsync(string content, string exerciseType, string level, int questionCount)
    {
        try
        {
            var prompt = BuildPrompt(content, exerciseType, level, questionCount);
            var response = await CallGeminiApiAsync(prompt);
            return response?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting raw Gemini response: {Message}", ex.Message);
            return null;
        }
    }

    private async Task<GeminiResponse?> CallGeminiApiAsync(string prompt)
    {
        var requestBody = new GeminiRequest
        {
            Contents = new List<Content>
            {
                new Content
                {
                    Parts = new List<Part>
                    {
                        new Part { Text = prompt }
                    }
                }
            },
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0.3,
                MaxOutputTokens = 4096,
                TopK = 20,
                TopP = 0.8
            }
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });

        var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
        var url = $"{_baseUrl}?key={_apiKey}";

        var response = await _httpClient.PostAsync(url, requestContent);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Gemini API Response: {Response}", responseContent);
            
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                _logger.LogWarning("Gemini API returned empty response content");
                return null;
            }
            
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            _logger.LogInformation("Deserialized response - Candidates count: {Count}", geminiResponse?.Candidates?.Count ?? 0);
            
            if (geminiResponse?.Candidates == null || !geminiResponse.Candidates.Any())
            {
                _logger.LogWarning("Gemini API returned empty candidates");
                return geminiResponse;
            }
            
            var firstCandidate = geminiResponse.Candidates.First();
            _logger.LogInformation("First candidate finish reason: {FinishReason}", firstCandidate.FinishReason);
            _logger.LogInformation("First candidate content parts count: {Count}", firstCandidate.Content?.Parts?.Count ?? 0);
            
            if (firstCandidate.FinishReason == "MAX_TOKENS")
            {
                _logger.LogWarning("Gemini response was truncated due to MAX_TOKENS limit");
            }
            
            if (firstCandidate.Content?.Parts?.Any() == true)
            {
                var firstPart = firstCandidate.Content.Parts.First();
                _logger.LogInformation("First part text length: {Length}, Text preview: {Preview}", 
                    firstPart.Text?.Length ?? 0, 
                    firstPart.Text?.Substring(0, Math.Min(200, firstPart.Text?.Length ?? 0)));
            }
            
            return geminiResponse;
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
        return null;
    }

    private string BuildPrompt(string content, string exerciseType, string level, int questionCount)
    {
        var difficultyGuide = level switch
        {
            "Beginner" => "Use simple vocabulary and basic grammar structures appropriate for TOEIC beginners",
            "Intermediate" => "Use moderate vocabulary and standard grammar patterns typical in TOEIC intermediate level", 
            "Advanced" => "Use complex vocabulary and advanced grammar structures found in TOEIC advanced level",
            _ => "Use appropriate vocabulary for the TOEIC level"
        };

        var typeGuide = exerciseType switch
        {
            "Part 5" => @"
Create individual sentences with one blank each. Focus on:
- Grammar: verb tenses, word forms (noun/adj/adv/verb)
- Vocabulary: prepositions, conjunctions, articles

Example format:
The manager _____ the report by Friday.
(A) complete (B) completes (C) completing (D) will complete",

            "Part 6" => @"
Create a complete business document (email, letter, notice) with exactly 4 blanks numbered (1), (2), (3), (4).
Each blank tests grammar, vocabulary, or sentence connections in context.

Format: Create 4 separate questions, each referring to one blank in the passage.",

            "Part 7" => @"
Create reading comprehension questions based on a complete passage.
Questions test: main ideas, details, inferences, vocabulary in context.",

            _ => "Create appropriate TOEIC multiple choice questions based on the content following official TOEIC format"
        };

        if (exerciseType == "Part 6")
        {
            return $@"Create TOEIC Part 6 text completion exercise about: {content}

Create a business email/letter with exactly 4 blanks. {difficultyGuide}

Return JSON with 4 questions referring to the same passage:
[
  {{""questionText"":""Dear Team, The meeting (1) _____ be held tomorrow."",""options"":[""will"",""would"",""should"",""might""],""correctAnswer"":0,""explanation"":""Future plan"",""difficulty"":3}},
  {{""questionText"":""Please (2) _____ your attendance by email."",""options"":[""confirm"",""confirmed"",""confirming"",""confirmation""],""correctAnswer"":0,""explanation"":""Imperative verb"",""difficulty"":3}},
  {{""questionText"":""The meeting will start (3) _____ 2 PM."",""options"":[""in"",""on"",""at"",""by""],""correctAnswer"":2,""explanation"":""Time preposition"",""difficulty"":3}},
  {{""questionText"":""We look forward (4) _____ seeing everyone."",""options"":[""to"",""for"",""at"",""in""],""correctAnswer"":0,""explanation"":""Phrasal verb"",""difficulty"":3}}
]";
        }

        if (exerciseType == "Part 7")
        {
            string passageGuide = level switch
            {
                "Beginner" => "Create ONE short business document (100-150 words) like a simple email, advertisement, or notice",
                "Intermediate" => "Create ONE longer business document (200-300 words) like a detailed report, article, or professional correspondence", 
                "Advanced" => "Create TWO related business documents (150-200 words each) like email + attachment, article + chart, or correspondence sequence",
                _ => "Create appropriate business document(s) for reading comprehension"
            };

            return $@"Create TOEIC Part 7 reading comprehension exercise about: {content}

{passageGuide}. {difficultyGuide}

Generate 4 comprehension questions that test:
- Main idea and specific details
- Purpose and inference  
- Vocabulary in context
- Cross-reference (for Advanced level with 2 passages)

Return JSON with complete passage(s) and 4 questions:
[
  {{""questionText"":""What is the main purpose of this document?"",""options"":[""To inform about policy"",""To request information"",""To schedule a meeting"",""To announce an event""],""correctAnswer"":0,""explanation"":""The document clearly states its main purpose"",""difficulty"":3}},
  {{""questionText"":""According to the passage, when will the event take place?"",""options"":[""Next Monday"",""Next Wednesday"",""Next Friday"",""Next Sunday""],""correctAnswer"":1,""explanation"":""The date is mentioned in paragraph 2"",""difficulty"":3}},
  {{""questionText"":""The word 'comprehensive' in paragraph 1 is closest in meaning to:"",""options"":[""expensive"",""complete"",""complicated"",""convenient""],""correctAnswer"":1,""explanation"":""Comprehensive means complete and thorough"",""difficulty"":3}},
  {{""questionText"":""What can be inferred about the company's policy?"",""options"":[""It is flexible"",""It is strict"",""It is new"",""It is temporary""],""correctAnswer"":2,""explanation"":""The passage mentions recent changes"",""difficulty"":3}}
]

Make sure to include the complete reading passage(s) as content before the questions.";
        }

        return $@"Create {questionCount} English multiple choice questions for TOEIC {exerciseType} based on this content: {content}

Requirements:
- {difficultyGuide}
- {typeGuide}
- Each question has 4 options (A), (B), (C), (D)
- Only one correct answer
- Include explanation

Return as JSON array:
[
  {{
    ""questionText"": ""Question with _____ blank if needed"",
    ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
    ""correctAnswer"": 0,
    ""explanation"": ""Brief explanation"",
    ""difficulty"": 3
  }}
]";
    }



    private List<GeneratedQuestion> ParseGeneratedQuestions(string generatedText)
    {
        try
        {
            // Clean up the response text and try to find valid JSON
            var cleanedText = generatedText.Trim();
            
            // Look for JSON in markdown code blocks first
            var jsonBlockStart = cleanedText.IndexOf("```json");
            var jsonBlockEnd = cleanedText.LastIndexOf("```");
            
            if (jsonBlockStart >= 0 && jsonBlockEnd > jsonBlockStart)
            {
                // Extract JSON from markdown code block
                var startIndex = jsonBlockStart + 7; // length of "```json"
                var endIndex = jsonBlockEnd;
                var jsonText = cleanedText.Substring(startIndex, endIndex - startIndex).Trim();
                
                // Try to parse as Part 7 format first (with passages and questions)
                try
                {
                    var part7Response = JsonSerializer.Deserialize<Part7Response>(jsonText, new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        AllowTrailingCommas = true
                    });
                    
                    if (part7Response?.Questions != null)
                    {
                        return part7Response.Questions.Where(q => !string.IsNullOrEmpty(q.QuestionText)).ToList();
                    }
                }
                catch
                {
                    // If Part 7 format fails, try mixed format (array with passage and questions)
                    try
                    {
                        var mixedResponse = JsonSerializer.Deserialize<List<JsonElement>>(jsonText, new JsonSerializerOptions 
                        { 
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            AllowTrailingCommas = true
                        });
                        
                        if (mixedResponse != null)
                        {
                            var parsedQuestions = new List<GeneratedQuestion>();
                            foreach (var item in mixedResponse)
                            {
                                if (item.TryGetProperty("questionText", out var questionTextProperty))
                                {
                                    var question = JsonSerializer.Deserialize<GeneratedQuestion>(item.GetRawText(), new JsonSerializerOptions 
                                    { 
                                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                        AllowTrailingCommas = true
                                    });
                                    if (question != null && !string.IsNullOrEmpty(question.QuestionText))
                                    {
                                        parsedQuestions.Add(question);
                                    }
                                }
                            }
                            if (parsedQuestions.Any())
                            {
                                return parsedQuestions;
                            }
                        }
                    }
                    catch
                    {
                        // Continue to regular question list format
                    }
                }
                
                // Try regular question list format
                var questions = JsonSerializer.Deserialize<List<GeneratedQuestion>>(jsonText, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    AllowTrailingCommas = true
                });
                
                return questions?.Where(q => !string.IsNullOrEmpty(q.QuestionText)).ToList() ?? new List<GeneratedQuestion>();
            }
            
            // Try to extract JSON from the response (fallback)
            var jsonStart = cleanedText.IndexOf('[');
            var jsonEnd = cleanedText.LastIndexOf(']');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonText = cleanedText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                
                // Clean up common JSON issues from Gemini
                jsonText = jsonText
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Replace("\n", " ")
                    .Replace("\r", "")
                    .Trim();
                
                // Try to fix incomplete JSON by counting brackets
                var openBrackets = jsonText.Count(c => c == '{');
                var closeBrackets = jsonText.Count(c => c == '}');
                var openArrays = jsonText.Count(c => c == '[');
                var closeArrays = jsonText.Count(c => c == ']');
                
                // Add missing closing brackets/braces if needed
                while (closeBrackets < openBrackets)
                {
                    jsonText += "}";
                    closeBrackets++;
                }
                while (closeArrays < openArrays)
                {
                    jsonText += "]";
                    closeArrays++;
                }
                
                var questions = JsonSerializer.Deserialize<List<GeneratedQuestion>>(jsonText, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    AllowTrailingCommas = true
                });
                
                return questions?.Where(q => !string.IsNullOrEmpty(q.QuestionText)).ToList() ?? new List<GeneratedQuestion>();
            }

            // Fallback: manual parsing if JSON format fails
            return ParseQuestionsManually(generatedText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing generated questions");
            return ParseQuestionsManually(generatedText);
        }
    }

    private List<GeneratedQuestion> ParseQuestionsManually(string text)
    {
        // Simple fallback parsing - create at least one question from the generated text
        var questions = new List<GeneratedQuestion>();
        
        // This is a basic implementation - you can enhance it based on Gemini's actual response format
        if (!string.IsNullOrWhiteSpace(text))
        {
            questions.Add(new GeneratedQuestion
            {
                QuestionText = "Generated question based on the content (manual parsing)",
                Options = new List<string> { "Option A", "Option B", "Option C", "Option D" },
                CorrectAnswer = 0,
                Explanation = "This question was generated using manual parsing",
                Difficulty = 3
            });
        }

        return questions;
    }
}

public class Part7Response
{
    public List<Part7Passage>? Passages { get; set; }
    public List<GeneratedQuestion>? Questions { get; set; }
}

public class Part7Passage
{
    public string? Title { get; set; }
    public string? Content { get; set; }
}