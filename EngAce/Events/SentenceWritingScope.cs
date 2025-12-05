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
      ""CorrectAnswer"": ""The correct English translation of the Vietnamese sentence"",
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

**CRITICAL:**
- **CorrectAnswer** is REQUIRED for each sentence - this is the ideal English translation
- Provide natural, accurate translations appropriate for the CEFR level
- Use PascalCase for all property names
- Ensure all JSON is properly formatted and valid";

        public const string ReviewInstruction = @"
## **YOUR ROLE:**
You are an **Expert English Writing Tutor** specializing in helping Vietnamese learners improve their sentence translation and writing skills.

## **YOUR TASK:**
Evaluate a Vietnamese-to-English translation. Provide detailed, educational feedback.

## **EVALUATION CRITERIA:**

### **Score (0-10):**
- **9-10:** Excellent - Natural, accurate, appropriate level
- **7-8:** Good - Minor issues, mostly correct
- **5-6:** Acceptable - Several errors but meaning clear
- **3-4:** Needs improvement - Significant errors
- **0-2:** Poor - Major errors, meaning unclear

### **REQUIRED FEEDBACK:**

1. **correctAnswer**: The correct/ideal English translation
   - **CRITICAL:** ALWAYS provide a complete, correct English translation
   - Even if the student's answer is perfect (score 9-10), still provide the correct answer
   - This helps students verify their understanding
   - Should be natural and appropriate for learner's level
   - Never leave this empty
   
2. **vocabulary**: Key vocabulary in the sentence (3-5 words)
   - Format: ""word: nghĩa tiếng Việt""
   - Example: ""travel: du lịch; enjoy: thích thú; beautiful: đẹp""
   - Separate words by semicolon (;)
   
3. **grammarPoints**: Grammar structures used in this sentence
   - Explain what grammar is needed
   - Be educational and concise
   - Example: ""Present simple tense (I go...), Adjective + noun (beautiful place)""
   
4. **errorExplanation**: What was wrong (ONLY if score < 7)
   - Be specific: point out the exact errors
   - Explain why it's wrong
   - Suggest how to fix it
   - Use empty string """" if score >= 7

## **OUTPUT FORMAT:**

Return valid JSON:

```json
{
  ""score"": 8,
  ""correctAnswer"": ""The correct English translation"",
  ""vocabulary"": ""word1: nghĩa1; word2: nghĩa2; word3: nghĩa3"",
  ""grammarPoints"": ""Grammar structures explanation"",
  ""errorExplanation"": ""Detailed error explanation (empty if score >= 7)""
}
```

**Important:**
- correctAnswer: ALWAYS provide
- vocabulary: ALWAYS provide 3-5 words with Vietnamese meanings
- grammarPoints: ALWAYS explain the grammar
- errorExplanation: Only provide if score < 7";

        public static async Task<SentenceResponse> GenerateSentences(string apiKey, EnglishLevel level, string topic, int sentenceCount, string writingStyle = "Communicative")
        {
            try
            {
                var userLevel = GeneralHelper.GetEnumDescription(level);
                var promptBuilder = new StringBuilder();

                promptBuilder.AppendLine($"## Input Information:");
                promptBuilder.AppendLine($"- **Topic:** {topic}");
                promptBuilder.AppendLine($"- **CEFR Level:** {userLevel}");
                promptBuilder.AppendLine($"- **Number of sentences:** {sentenceCount}");
                promptBuilder.AppendLine($"- **Writing Style:** {writingStyle}");
                promptBuilder.AppendLine();
                promptBuilder.AppendLine("## Level Description:");
                promptBuilder.AppendLine(GetLevelDescription(level));
                promptBuilder.AppendLine();
                promptBuilder.AppendLine(GetWritingStyleDescription(writingStyle));
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
                                        CorrectAnswer = new { type = "string" },
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
                                    required = new[] { "Id", "Vietnamese", "CorrectAnswer" }
                                }
                            }
                        },
                        required = new[] { "Sentences" }
                    })
                    .DisableAllSafetySettings()
                    .Build();

                var response = await generator.GenerateContentAsync(apiRequest, ModelVersion.Gemini_20_Flash_Lite);
                
                Console.WriteLine($"[DEBUG] Raw response from Gemini:");
                Console.WriteLine(response.Result);
                Console.WriteLine($"[DEBUG] Response length: {response.Result?.Length ?? 0}");
                
                var result = JsonConvert.DeserializeObject<SentenceResponse>(response.Result);
                
                Console.WriteLine($"[DEBUG] Deserialized result: {(result != null ? "Success" : "Null")}");
                Console.WriteLine($"[DEBUG] Sentences in result: {result?.Sentences?.Count ?? 0}");
                
                if (result != null && result.Sentences != null)
                {
                    foreach (var sentence in result.Sentences)
                    {
                        Console.WriteLine($"  - Sentence {sentence.Id}: {sentence.Vietnamese}");
                    }
                }
                
                return result ?? new SentenceResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception in GenerateSentences:");
                Console.WriteLine($"  - Message: {ex.Message}");
                Console.WriteLine($"  - StackTrace: {ex.StackTrace}");
                Console.WriteLine($"  - InnerException: {ex.InnerException?.Message}");
                
                // Check if it's a rate limit error
                if (ex.Message.Contains("429") || ex.Message.Contains("RATE_LIMIT_EXCEEDED") || ex.Message.Contains("Quota exceeded"))
                {
                    throw new Exception("RATE_LIMIT: API đã vượt quá giới hạn request. Vui lòng đợi 1 phút rồi thử lại.");
                }
                
                throw new Exception($"Lỗi khi tạo câu: {ex.Message}");
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
                        correctAnswer = new { type = "string" },
                        vocabulary = new { type = "string" },
                        grammarPoints = new { type = "string" },
                        errorExplanation = new { type = "string" }
                    },
                    required = new[] { "score", "correctAnswer", "vocabulary", "grammarPoints", "errorExplanation" }
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

        private static string GetWritingStyleDescription(string writingStyle)
        {
            return writingStyle.ToLower() switch
            {
                "academic" => @"## Writing Style: Academic
- Focus on **formal language** and **academic vocabulary**
- Use **complex sentence structures** and **formal expressions**
- Include **analytical** and **descriptive** language
- Sentences suitable for essays, reports, and academic discussions
- Example contexts: research, analysis, formal opinions, academic arguments",
                _ => @"## Writing Style: Communicative (Conversational)
- Focus on **everyday language** and **practical communication**
- Use **natural expressions** and **common phrases**
- Include **casual** and **conversational** language
- Sentences suitable for daily conversations, chats, and informal interactions
- Example contexts: daily life, shopping, socializing, personal experiences"
            };
        }
    }
}
