using EngAce.Api.DTO;
using Entities;
using Entities.Enums;
using Events;
using Helper;
using Microsoft.AspNetCore.Mvc;
using EngAce.Api.Services.Interfaces;
using System.Text;
using System.Text.Json;

namespace EngAce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SentenceWritingController(IGeminiService geminiService) : ControllerBase
    {
        private readonly IGeminiService _geminiService = geminiService;
        private readonly string _accessKey = HttpContextHelper.GetAccessKey();

        [HttpPost("Generate")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Client, NoStore = false)]
        public async Task<ActionResult<SentenceResponse>> Generate([FromBody] GenerateSentences request, string provider = "gemini")
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                return Unauthorized("Invalid Access Key");
            }

            var topic = request.Topic.Trim();

            if (string.IsNullOrWhiteSpace(topic))
            {
                return BadRequest("Chủ đề không được để trống.");
            }

            if (GeneralHelper.GetTotalWords(topic) > SentenceWritingScope.MaxTopicWords)
            {
                return BadRequest($"Chủ đề không được chứa nhiều hơn {SentenceWritingScope.MaxTopicWords} từ.");
            }

            if (request.SentenceCount < SentenceWritingScope.MinSentences || request.SentenceCount > SentenceWritingScope.MaxSentences)
            {
                return BadRequest($"Số lượng câu phải nằm trong khoảng {SentenceWritingScope.MinSentences} đến {SentenceWritingScope.MaxSentences}.");
            }

            try
            {
                var writingStyle = string.IsNullOrEmpty(request.WritingStyle) ? "Communicative" : request.WritingStyle;
                
                var prompt = BuildSentenceWritingPrompt(request.Level, topic, request.SentenceCount, writingStyle);
                var response = await _geminiService.GenerateResponseAsync(prompt, provider, maxTokens: 4096);
                
                // Extract JSON from response
                var jsonContent = ExtractJsonFromResponse(response);
                
                // Parse to SentenceResponse
                var result = JsonSerializer.Deserialize<SentenceResponse>(jsonContent);
                
                if (result == null || result.Sentences == null || result.Sentences.Count == 0)
                {
                    return BadRequest("Không thể tạo câu luyện tập. Vui lòng thử lại.");
                }
                
                return Created("Success", result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Exception in Generate: {ex.Message}");
                Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                Console.WriteLine($"[ERROR] Inner exception: {ex.InnerException?.Message}");
                
                // Check if it's a rate limit error
                if (ex.Message.Contains("RATE_LIMIT") || ex.Message.Contains("429") || ex.Message.Contains("Quota exceeded"))
                {
                    return StatusCode(429, "🕐 API đang bận. Vui lòng đợi 1-2 phút và thử lại. Nếu vẫn lỗi, hãy kiểm tra Gemini API key quota.");
                }
                
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

        [HttpPost("Review")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Client, NoStore = false)]
        public async Task<ActionResult<object>> Review([FromBody] GenerateComment request)
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                return Unauthorized("Invalid Access Key");
            }

            var content = request.Content.Trim();

            if (GeneralHelper.GetTotalWords(content) < SentenceWritingScope.MinWordsPerSentence)
            {
                return BadRequest($"Câu phải dài tối thiểu {SentenceWritingScope.MinWordsPerSentence} từ.");
            }

            if (GeneralHelper.GetTotalWords(content) > SentenceWritingScope.MaxWordsPerSentence)
            {
                return BadRequest($"Câu không được dài hơn {SentenceWritingScope.MaxWordsPerSentence} từ.");
            }

            try
            {
                var result = await SentenceWritingScope.GenerateReview(_accessKey, request.UserLevel, request.Requirement, request.Content);
                return Ok(result);
            }
            catch
            {
                return Created("Success", "## CẢNH BÁO\n EngBuddy đang bận đi pha cà phê nên tạm thời vắng mặt. Cục cưng vui lòng ngồi chơi 3 phút rồi gửi lại cho EngBuddy nhận xét nha.\nYêu cục cưng nhiều lắm luôn á!");
            }
        }

        private static string BuildSentenceWritingPrompt(EnglishLevel level, string topic, int sentenceCount, string writingStyle)
        {
            var prompt = new StringBuilder();
            
            prompt.AppendLine(@"## **YOUR ROLE:**
You are an **Expert Vietnamese-English Language Teacher** specializing in creating practical learning materials.

## **YOUR TASK:**
Generate Vietnamese sentences for translation practice related to the given topic and CEFR level.

## **REQUIREMENTS:**
1. Sentences must be natural, practical, and commonly used
2. Match the CEFR level in vocabulary and grammar complexity
3. Include diverse structures (statements, questions, expressions)
4. Each sentence should be standalone

## **OUTPUT FORMAT:**
Return valid JSON with PascalCase property names:

```json
{
  ""Sentences"": [
    {
      ""Id"": 1,
      ""Vietnamese"": ""Câu tiếng Việt tự nhiên"",
      ""CorrectAnswer"": ""The correct English translation"",
      ""Suggestion"": {
        ""Vocabulary"": [
          {
            ""Word"": ""từ khóa"",
            ""Meaning"": ""key word""
          }
        ],
        ""Structure"": ""Sentence structure hint""
      }
    }
  ]
}
```

**CRITICAL:** CorrectAnswer is REQUIRED. Provide natural, accurate translations.");

            prompt.AppendLine();
            prompt.AppendLine($"## **Task Details:**");
            prompt.AppendLine($"- **Topic:** {topic}");
            prompt.AppendLine($"- **CEFR Level:** {level}");
            prompt.AppendLine($"- **Number of sentences:** {sentenceCount}");
            prompt.AppendLine($"- **Writing Style:** {writingStyle}");
            prompt.AppendLine();
            prompt.AppendLine("Generate the sentences now in valid JSON format:");

            return prompt.ToString();
        }

        private static string ExtractJsonFromResponse(string response)
        {
            var content = response.Trim();
            
            if (content.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                var startIndex = content.IndexOf('\n') + 1;
                var endIndex = content.LastIndexOf("```");
                
                if (startIndex > 0 && endIndex > startIndex)
                {
                    content = content.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }
            else if (content.StartsWith("```"))
            {
                var startIndex = content.IndexOf('\n') + 1;
                var endIndex = content.LastIndexOf("```");
                
                if (startIndex > 0 && endIndex > startIndex)
                {
                    content = content.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }
            
            return content;
        }
    }
}
