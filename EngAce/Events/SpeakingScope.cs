using Entities;
using Entities.Enums;
using Gemini.NET;
using Gemini.NET.Helpers;
using Helper;
using Models.Enums;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Events;

/// <summary>
/// Xử lý logic cho bài tập nói
/// </summary>
public class SpeakingScope
{
    private const string SpeakingCoachInstruction = "You are an award-winning ESL speaking coach who creates engaging speaking prompts for Vietnamese learners.";
    private const string SpeakingAnalysisInstruction = "You are an experienced IELTS speaking examiner who provides structured Vietnamese feedback.";

    private readonly string _apiKey;

    public SpeakingScope()
    {
        _apiKey = HttpContextHelper.GetGeminiApiKey();
    }

    /// <summary>
    /// Tạo bài tập nói với AI
    /// </summary>
    public async Task<SpeakingExercise> GenerateExerciseAsync(
        SpeakingTopic topic,
        EnglishLevel level,
        string? customTopic,
        AiModel aiModel = AiModel.GeminiFlashLite)
    {
        var topicDescription = GetTopicDescription(topic);
        var levelDescription = GeneralHelper.GetLevelDescription(level);
        var customInstruction = string.IsNullOrWhiteSpace(customTopic) 
            ? "" 
            : $"\n- Custom specific topic: {customTopic}";

        var prompt = $@"Generate a speaking exercise for English learners with the following requirements:
- Topic: {topicDescription}{customInstruction}
- English Level: {levelDescription}
- The exercise should include:
  1. A clear title
  2. A speaking prompt (a scenario or question that requires the learner to speak, 40-80 words)
  3. A helpful hint or guideline for the learner

Return ONLY a valid JSON object (without markdown formatting) with this structure:
{{
  ""title"": ""Exercise title"",
  ""prompt"": ""The speaking prompt or scenario"",
  ""hint"": ""Helpful tips for the learner""
}}

Make the content natural, engaging, and appropriate for the specified level.
The prompt should encourage the learner to speak for 30-60 seconds.";

        var responseSchema = new
        {
            type = "object",
            properties = new
            {
                title = new { type = "string" },
                prompt = new { type = "string" },
                hint = new { type = "string" }
            },
            required = new[] { "title", "prompt", "hint" }
        };

        string sanitized;

        if (aiModel == AiModel.Gpt5Preview)
        {
            var raw = await Gpt5Client.GenerateJsonResponseAsync(
                SpeakingCoachInstruction,
                prompt,
                responseSchema,
                temperature: 0.5f,
                schemaName: "speaking_exercise");

            sanitized = SanitizeModelPayload(raw);
        }
        else
        {
            var apiRequest = new ApiRequestBuilder()
                .WithPrompt(prompt)
                .WithResponseSchema(responseSchema)
                .DisableAllSafetySettings()
                .Build();

            var generator = new Generator(_apiKey);
            var response = await generator.GenerateContentAsync(apiRequest, ModelVersion.Gemini_20_Flash_Lite);
            sanitized = SanitizeModelPayload(response.Result);
        }

        SpeakingExerciseData? exerciseData;
        try
        {
            exerciseData = JsonHelper.AsObject<SpeakingExerciseData>(sanitized);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Không thể phân tích dữ liệu phản hồi từ mô hình.", ex);
        }

        if (exerciseData == null)
        {
            throw new Exception("Failed to parse speaking exercise data");
        }

        return new SpeakingExercise
        {
            ExerciseId = Guid.NewGuid().ToString(),
            Topic = topic,
            EnglishLevel = level,
            Title = exerciseData.Title,
            Prompt = exerciseData.Prompt,
            Hint = exerciseData.Hint,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Phân tích giọng nói và ngữ pháp với AI
    /// </summary>
    public async Task<SpeakingAnalysis> AnalyzeSpeechAsync(
        string transcribedText,
        string originalPrompt,
        AiModel aiModel = AiModel.GeminiFlashLite)
    {
        var prompt = $@"Analyze the following English speech transcription and provide detailed feedback.

Original Prompt: {originalPrompt}

Transcribed Speech: {transcribedText}

Provide a comprehensive analysis in JSON format (without markdown formatting) with this structure:
{{
  ""overallScore"": 85.5,
  ""pronunciationScore"": 90.0,
  ""grammarScore"": 80.0,
  ""vocabularyScore"": 85.0,
  ""fluencyScore"": 88.0,
  ""grammarErrors"": [
    {{
      ""startIndex"": 10,
      ""endIndex"": 15,
      ""errorText"": ""the error text"",
      ""errorType"": ""grammar"",
      ""description"": ""Brief description of the error"",
      ""correction"": ""Corrected version"",
      ""explanationInVietnamese"": ""Giải thích bằng tiếng Việt""
    }}
  ],
  ""overallFeedback"": ""General feedback in Vietnamese"",
  ""suggestions"": [
    ""Suggestion 1 in Vietnamese"",
    ""Suggestion 2 in Vietnamese""
  ]
}}

Important guidelines:
- Scores should be between 0-100
- Identify grammar, vocabulary, and structural errors
- Provide corrections and explanations in Vietnamese
- Give constructive feedback in Vietnamese
- Consider the context of the original prompt
- If the speech is completely off-topic, note that in the feedback
- Pronunciation score should reflect clarity and naturalness based on the text quality";

        var responseSchema = new
        {
            type = "object",
            properties = new
            {
                overallScore = new { type = "number" },
                pronunciationScore = new { type = "number" },
                grammarScore = new { type = "number" },
                vocabularyScore = new { type = "number" },
                fluencyScore = new { type = "number" },
                grammarErrors = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            startIndex = new { type = "integer" },
                            endIndex = new { type = "integer" },
                            errorText = new { type = "string" },
                            errorType = new { type = "string" },
                            description = new { type = "string" },
                            correction = new { type = "string" },
                            explanationInVietnamese = new { type = "string" }
                        }
                    }
                },
                overallFeedback = new { type = "string" },
                suggestions = new
                {
                    type = "array",
                    items = new { type = "string" }
                }
            },
            required = new[] { "overallScore", "pronunciationScore", "grammarScore", "vocabularyScore", "fluencyScore", "grammarErrors", "overallFeedback", "suggestions" }
        };

        string sanitized;

        if (aiModel == AiModel.Gpt5Preview)
        {
            var raw = await Gpt5Client.GenerateJsonResponseAsync(
                SpeakingAnalysisInstruction,
                prompt,
                responseSchema,
                temperature: 0.3f,
                schemaName: "speaking_analysis");

            sanitized = SanitizeModelPayload(raw);
        }
        else
        {
            var apiRequest = new ApiRequestBuilder()
                .WithPrompt(prompt)
                .WithResponseSchema(responseSchema)
                .DisableAllSafetySettings()
                .Build();

            var generator = new Generator(_apiKey);
            var response = await generator.GenerateContentAsync(apiRequest, ModelVersion.Gemini_20_Flash_Lite);
            sanitized = SanitizeModelPayload(response.Result);
        }

        SpeakingAnalysisData? analysisData;
        try
        {
            analysisData = JsonHelper.AsObject<SpeakingAnalysisData>(sanitized);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Không thể phân tích dữ liệu phản hồi từ mô hình.", ex);
        }

        if (analysisData == null)
        {
            throw new Exception("Failed to parse analysis data");
        }

        return new SpeakingAnalysis
        {
            TranscribedText = transcribedText,
            OverallScore = analysisData.OverallScore,
            PronunciationScore = analysisData.PronunciationScore,
            GrammarScore = analysisData.GrammarScore,
            VocabularyScore = analysisData.VocabularyScore,
            FluencyScore = analysisData.FluencyScore,
            GrammarErrors = analysisData.GrammarErrors.Select(e => new GrammarError
            {
                StartIndex = e.StartIndex,
                EndIndex = e.EndIndex,
                ErrorText = e.ErrorText,
                ErrorType = e.ErrorType,
                Description = e.Description,
                Correction = e.Correction,
                ExplanationInVietnamese = e.ExplanationInVietnamese
            }).ToList(),
            OverallFeedback = analysisData.OverallFeedback,
            Suggestions = analysisData.Suggestions
        };
    }

    private string SanitizeModelPayload(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return raw;
        }

        // Remove markdown code fences
        var pattern = @"^```(?:json)?\s*|\s*```$";
        var cleaned = Regex.Replace(raw.Trim(), pattern, "", RegexOptions.Multiline);

        // Extract JSON object if wrapped in other text
        var jsonMatch = Regex.Match(cleaned, @"\{[\s\S]*\}");
        if (jsonMatch.Success)
        {
            cleaned = jsonMatch.Value;
        }

        return cleaned.Trim();
    }

    private string GetTopicDescription(SpeakingTopic topic)
    {
        return topic switch
        {
            SpeakingTopic.SelfIntroduction => "Self-introduction and personal information",
            SpeakingTopic.DailyLife => "Daily life activities and routines",
            SpeakingTopic.HobbiesAndInterests => "Hobbies, interests, and leisure activities",
            SpeakingTopic.TravelAndExploration => "Travel experiences and exploration",
            SpeakingTopic.WorkAndCareer => "Work, career, and professional life",
            SpeakingTopic.EducationAndLearning => "Education, learning, and academic topics",
            SpeakingTopic.TechnologyAndFuture => "Technology, innovation, and future trends",
            _ => "General conversation"
        };
    }

    // Helper classes for JSON deserialization
    private class SpeakingExerciseData
    {
        public string Title { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string Hint { get; set; } = string.Empty;
    }

    private class SpeakingAnalysisData
    {
        public double OverallScore { get; set; }
        public double PronunciationScore { get; set; }
        public double GrammarScore { get; set; }
        public double VocabularyScore { get; set; }
        public double FluencyScore { get; set; }
        public List<GrammarErrorData> GrammarErrors { get; set; } = new();
        public string OverallFeedback { get; set; } = string.Empty;
        public List<string> Suggestions { get; set; } = new();
    }

    private class GrammarErrorData
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string ErrorText { get; set; } = string.Empty;
        public string ErrorType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Correction { get; set; } = string.Empty;
        public string ExplanationInVietnamese { get; set; } = string.Empty;
    }
}
