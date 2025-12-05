using EngAce.Api.DTO;
using Entities;
using Entities.Enums;
using Events;
using Helper;
using Microsoft.AspNetCore.Mvc;

namespace EngAce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SentenceWritingController() : ControllerBase
    {
        private readonly string _accessKey = HttpContextHelper.GetAccessKey();

        [HttpPost("Generate")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Client, NoStore = false)]
        public async Task<ActionResult<SentenceResponse>> Generate([FromBody] GenerateSentences request)
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
                
                Console.WriteLine($"[DEBUG] Calling GenerateSentences with:");
                Console.WriteLine($"  - Access Key: {(!string.IsNullOrEmpty(_accessKey) ? "Present" : "Missing")}");
                Console.WriteLine($"  - Level: {request.Level}");
                Console.WriteLine($"  - Topic: {topic}");
                Console.WriteLine($"  - SentenceCount: {request.SentenceCount}");
                Console.WriteLine($"  - WritingStyle: {writingStyle}");
                
                var result = await SentenceWritingScope.GenerateSentences(_accessKey, request.Level, topic, request.SentenceCount, writingStyle);
                
                Console.WriteLine($"[DEBUG] GenerateSentences returned: {(result != null ? "Success" : "Null")}");
                Console.WriteLine($"[DEBUG] Sentences count: {result?.Sentences?.Count ?? 0}");
                
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
    }
}
