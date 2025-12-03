using Entities;
using Entities.Enums;
using Gemini.NET;
using Helper;
using Models.Enums;
using Newtonsoft.Json;
using System.Text;

namespace Events
{
    public static class SentenceWritingScope
    {
        public const sbyte MaxTopicWords = 10;
        public const sbyte MinSentences = 5;
        public const sbyte MaxSentences = 20;
        public const sbyte MinWordsPerSentence = 3;
        public const short MaxWordsPerSentence = 50;

        public const string GenerateInstruction = @"
## **YOUR ROLE:**
You are an **Expert Vietnamese-English Language Teacher** specializing in creating practical and contextual learning materials for Vietnamese learners.

## **YOUR TASK:**
Generate a list of Vietnamese sentences related to a specific topic that are appropriate for the given CEFR level. These sentences will be used for translation practice.

## **REQUIREMENTS:**

### 1. **Sentence Characteristics:**
   - Each sentence must be **natural, practical, and commonly used** in real conversations
   - Sentences should be **diverse in structure** (statements, questions, expressions)
   - **Match the CEFR level** in terms of vocabulary complexity and grammar structures
   - Include various contexts: daily life, work, travel, opinions, etc.
   - Each sentence should be **standalone** (can be understood independently)

### 2. **Vocabulary & Grammar:**
   - Use vocabulary appropriate for the CEFR level
   - Include useful expressions and common phrases
   - Vary sentence lengths (short to medium)
   - Mix simple and complex structures based on level

### 3. **AI Suggestions (Optional but Helpful):**
   For each sentence, you may provide:
   - **Key vocabulary** with English meanings
   - **Suggested sentence structure** hints
   - These suggestions help learners when they struggle

## **OUTPUT FORMAT:**

Return valid JSON with this structure (use PascalCase for property names):

```json
{
  ""Sentences"": [
    {
      ""Id"": 1,
      ""Vietnamese"": ""Câu tiếng Việt tự nhiên và thực tế"",
      ""Suggestion"": {
        ""Vocabulary"": [
          {
            ""Word"": ""từ khóa 1"",
            ""Meaning"": ""key word 1""
          },
          {
            ""Word"": ""từ khóa 2"",
            ""Meaning"": ""key word 2""
          }
        ],
        ""Structure"": ""Gợi ý về cấu trúc câu tiếng Anh""
      }
    }
  ]
}
```

**Important:** Use PascalCase for all property names. Ensure all JSON is properly formatted and valid.";

        public const string ReviewInstruction = @"
## **YOUR ROLE:**
You are an **Expert English Writing Tutor** specializing in helping Vietnamese learners improve their sentence translation and writing skills.

## **YOUR TASK:**
Evaluate a Vietnamese-to-English translation provided by a learner. Provide constructive feedback focused on:
1. Accuracy of translation
2. Grammar correctness
3. Natural English expression
4. Appropriate vocabulary usage for the CEFR level

## **EVALUATION CRITERIA:**

### **Score (0-10):**
- **9-10:** Excellent - Natural, accurate, appropriate level
- **7-8:** Good - Minor issues, mostly correct
- **5-6:** Acceptable - Several errors but meaning clear
- **3-4:** Needs improvement - Significant errors
- **0-2:** Poor - Major errors, meaning unclear

### **Feedback Areas:**
1. **Comment:** Overall assessment and encouragement
2. **Grammar:** Specific grammar issues (if any)
3. **StructureTip:** How to improve sentence structure
4. **Suggestion:** A model translation for reference

## **OUTPUT FORMAT:**

Return valid JSON:

```json
{
  ""score"": 8,
  ""comment"": ""Nhận xét chung về bản dịch"",
  ""grammar"": ""Phân tích lỗi ngữ pháp (nếu có)"",
  ""structureTip"": ""Gợi ý cải thiện cấu trúc câu"",
  ""suggestion"": ""Câu gợi ý hoàn chỉnh""
}
```

**Important:**
- Be encouraging and constructive
- Explain errors clearly in Vietnamese
- Provide specific examples
- Suggest improvements, not just corrections";

        public static async Task<SentenceResponse> GenerateSentences(string apiKey, EnglishLevel level, string topic, int sentenceCount)
        {
            try
            {
                var userLevel = GeneralHelper.GetEnumDescription(level);
                var promptBuilder = new StringBuilder();

                promptBuilder.AppendLine($"## Input Information:");
                promptBuilder.AppendLine($"- **Topic:** {topic}");
                promptBuilder.AppendLine($"- **CEFR Level:** {userLevel}");
                promptBuilder.AppendLine($"- **Number of sentences:** {sentenceCount}");
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("## Level Description:");
                promptBuilder.AppendLine(GetLevelDescription(level));
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("Generate the sentences now.");

                var generator = new Generator(apiKey);

                var apiRequest = new ApiRequestBuilder()
                    .WithSystemInstruction(GenerateInstruction)
                    .WithPrompt(promptBuilder.ToString())
                    .WithDefaultGenerationConfig()
                    .WithResponseSchema(new
                    {
                        type = "object",
                        properties = new
                        {
                            Sentences = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        Id = new { type = "integer" },
                                        Vietnamese = new { type = "string" },
                                        Suggestion = new
                                        {
                                            type = "object",
                                            properties = new
                                            {
                                                Vocabulary = new
                                                {
                                                    type = "array",
                                                    items = new
                                                    {
                                                        type = "object",
                                                        properties = new
                                                        {
                                                            Word = new { type = "string" },
                                                            Meaning = new { type = "string" }
                                                        },
                                                        required = new[] { "Word", "Meaning" }
                                                    }
                                                },
                                                Structure = new { type = "string" }
                                            },
                                            required = new[] { "Vocabulary", "Structure" }
                                        }
                                    },
                                    required = new[] { "Id", "Vietnamese" }
                                }
                            }
                        },
                        required = new[] { "Sentences" }
                    })
                    .DisableAllSafetySettings()
                    .Build();

                var response = await generator.GenerateContentAsync(apiRequest, ModelVersion.Gemini_20_Flash_Lite);
                var result = JsonConvert.DeserializeObject<SentenceResponse>(response.Result);
                return result ?? new SentenceResponse();
            }
            catch
            {
                return new SentenceResponse { Sentences = new List<SentenceData>() };
            }
        }

        public static async Task<object> GenerateReview(string apiKey, EnglishLevel level, string requirement, string content)
        {
            var promptBuilder = new StringBuilder();

            promptBuilder.AppendLine("## **Vietnamese sentence (Original):**");
            promptBuilder.AppendLine(requirement.Trim());
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("## **Learner's English translation:**");
            promptBuilder.AppendLine(content.Trim());
            promptBuilder.AppendLine();
            promptBuilder.AppendLine($"## **Learner's CEFR level:** {GeneralHelper.GetEnumDescription(level)}");
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("Evaluate the translation now.");

            var generator = new Generator(apiKey);

            var apiRequest = new ApiRequestBuilder()
                .WithSystemInstruction(ReviewInstruction)
                .WithPrompt(promptBuilder.ToString())
                .WithDefaultGenerationConfig(1, 8192)
                .WithResponseSchema(new
                {
                    type = "object",
                    properties = new
                    {
                        score = new { type = "integer" },
                        comment = new { type = "string" },
                        grammar = new { type = "string" },
                        structureTip = new { type = "string" },
                        suggestion = new { type = "string" }
                    },
                    required = new[] { "score", "comment", "grammar", "structureTip", "suggestion" }
                })
                .DisableAllSafetySettings()
                .Build();

            var response = await generator.GenerateContentAsync(apiRequest, ModelVersion.Gemini_20_Flash);
            return JsonConvert.DeserializeObject<object>(response.Result) ?? new { };
        }

        private static string GetLevelDescription(EnglishLevel level)
        {
            return level switch
            {
                EnglishLevel.Beginner => "A1: Basic words and simple present tense. Short everyday phrases.",
                EnglishLevel.Elementary => "A2: Simple past, can/must, basic comparisons. Common daily situations.",
                EnglishLevel.Intermediate => "B1: Present perfect, conditionals, passive voice. Express opinions and experiences.",
                EnglishLevel.UpperIntermediate => "B2: Complex conditionals, phrasal verbs, advanced structures. Discuss abstract topics.",
                EnglishLevel.Advanced => "C1: Subtle expressions, idioms, advanced grammar. Professional and academic contexts.",
                EnglishLevel.Proficient => "C2: Native-like fluency, literary expressions, perfect accuracy. All contexts.",
                _ => string.Empty,
            };
        }
    }
}
