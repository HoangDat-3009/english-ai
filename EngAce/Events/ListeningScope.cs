using Entities;
using Entities.Enums;
using Gemini.NET;
using Gemini.NET.Helpers;
using Helper;
using Models.Enums;
using System.Text;

namespace Events
{
    public static class ListeningScope
    {
        public const sbyte MinTotalQuestions = 3;
        public const sbyte MaxTotalQuestions = 10;

        private const string Instruction = @"You are an award-winning ESL content creator who specializes in designing immersive listening comprehension lessons for Vietnamese learners. You always:
- Produce authentic, engaging scripts that sound natural to native speakers.
- Match the lexical and grammatical difficulty to the learner's CEFR level.
- Provide concise yet helpful Vietnamese explanations for every question.
- Return valid minified JSON with no trailing comments, Markdown, code fences, or additional text.
- Avoid Markdown characters such as `**` in the output content.
";

        public static async Task<ListeningExercise> GenerateExerciseAsync(string apiKey, ListeningGenre genre, EnglishLevel level, sbyte questionsCount, string? customTopic)
        {
            var promptBuilder = new StringBuilder();

            promptBuilder.AppendLine($"Learner level: {GeneralHelper.GetEnumDescription(level)} ({level}).");
            promptBuilder.AppendLine($"Desired listening genre: {GeneralHelper.GetEnumDescription(genre)}.");

            if (!string.IsNullOrWhiteSpace(customTopic))
            {
                promptBuilder.AppendLine($"Custom topic focus: {customTopic.Trim()}.");
            }

            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Please craft a single cohesive listening passage that:");
            promptBuilder.AppendLine("- Has a descriptive yet concise title (max 12 words).");
            promptBuilder.AppendLine("- Contains 180 to 260 English words suitable for the learner level.");
            promptBuilder.AppendLine("- Uses vocabulary and sentence structures typical for the selected genre.");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Then create " + questionsCount + " comprehension questions with exactly 4 answer choices each (index starts at 0). Ensure:");
            promptBuilder.AppendLine("- Questions are in English and target both gist and detail.");
            promptBuilder.AppendLine("- Options are concise, mutually exclusive, and plausible.");
            promptBuilder.AppendLine("- RightOptionIndex is accurate.");
            promptBuilder.AppendLine("- ExplanationInVietnamese gives a short reason (max 45 words).");

            var apiRequest = new ApiRequestBuilder()
                .WithSystemInstruction(Instruction)
                .WithPrompt(promptBuilder.ToString())
                .WithDefaultGenerationConfig(0.4F)
                .WithResponseSchema(new
                {
                    type = "object",
                    properties = new
                    {
                        Title = new { type = "string" },
                        Transcript = new { type = "string" },
                        Questions = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    Question = new { type = "string" },
                                    Options = new
                                    {
                                        type = "array",
                                        items = new { type = "string" },
                                        minItems = 4,
                                        maxItems = 4
                                    },
                                    RightOptionIndex = new { type = "integer" },
                                    ExplanationInVietnamese = new { type = "string" }
                                },
                                required = new[] { "Question", "Options", "RightOptionIndex", "ExplanationInVietnamese" }
                            }
                        }
                    },
                    required = new[] { "Title", "Transcript", "Questions" }
                })
                .DisableAllSafetySettings()
                .Build();

            var generator = new Generator(apiKey);
            var response = await generator.GenerateContentAsync(apiRequest, ModelVersion.Gemini_20_Flash_Lite);
            var sanitizedPayload = SanitizeModelPayload(response.Result);

            ListeningGenerationResult? result;

            try
            {
                result = JsonHelper.AsObject<ListeningGenerationResult>(sanitizedPayload);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể phân tích dữ liệu phản hồi từ mô hình.", ex);
            }

            if (result == null || string.IsNullOrWhiteSpace(result.Transcript) || result.Questions.Count == 0)
            {
                throw new InvalidOperationException("Không thể tạo bài nghe. Vui lòng thử lại sau.");
            }

            var sanitizedQuestions = result.Questions
                .Where(q => !string.IsNullOrWhiteSpace(q.Question) && q.Options.Count >= 4)
                .Take(questionsCount)
                .Select(q => new Quiz
                {
                    Question = q.Question.Replace("**", "'").Trim(),
                    Options = q.Options.Select(option => option.Replace("**", "'").Trim()).ToList(),
                    RightOptionIndex = Convert.ToSByte(Math.Clamp(q.RightOptionIndex, 0, q.Options.Count - 1)),
                    ExplanationInVietnamese = q.ExplanationInVietnamese.Replace("**", "'").Trim()
                })
                .ToList();

            if (sanitizedQuestions.Count == 0)
            {
                throw new InvalidOperationException("Không thể tạo câu hỏi phù hợp.");
            }

            return new ListeningExercise
            {
                Title = result.Title.Replace("**", "'").Trim(),
                Transcript = result.Transcript.Replace("**", "'").Trim(),
                Genre = genre,
                EnglishLevel = level,
                Questions = sanitizedQuestions
            };
        }

        private sealed class ListeningGenerationResult
        {
            public string Title { get; set; } = string.Empty;
            public string Transcript { get; set; } = string.Empty;
            public List<ListeningGeneratedQuestion> Questions { get; set; } = [];
        }

        private sealed class ListeningGeneratedQuestion
        {
            public string Question { get; set; } = string.Empty;
            public List<string> Options { get; set; } = [];
            public int RightOptionIndex { get; set; }
            public string ExplanationInVietnamese { get; set; } = string.Empty;
        }

        private static string SanitizeModelPayload(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var working = raw.Trim();

            if (working.StartsWith("```", StringComparison.Ordinal))
            {
                var firstLineBreak = working.IndexOf('\n');
                if (firstLineBreak >= 0)
                {
                    working = working[(firstLineBreak + 1)..];
                }

                var closingFence = working.LastIndexOf("```", StringComparison.Ordinal);
                if (closingFence >= 0)
                {
                    working = working[..closingFence];
                }

                working = working.Trim();
            }

            var startIndex = working.IndexOf('{');
            var endIndex = working.LastIndexOf('}');

            if (startIndex >= 0 && endIndex >= startIndex)
            {
                return working.Substring(startIndex, endIndex - startIndex + 1);
            }

            return working;
        }
    }
}
