using Entities;
using Entities.Enums;
using Events;
using Helper;
using Microsoft.AspNetCore.Mvc;

namespace EngAce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController(ILogger<ChatbotController> logger) : ControllerBase
    {
        private readonly ILogger<ChatbotController> _logger = logger;
        private readonly string _accessKey = HttpContextHelper.GetAccessKey();

        [HttpPost("GenerateAnswer")]
        public async Task<ActionResult<ChatResponse>> GenerateAnswer([FromBody] Conversation request, string username, string gender, sbyte age, EnglishLevel englishLevel, bool enableReasoning = false, bool enableSearching = false)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                return Ok("Gửi vội vậy cục cưng! Chưa nhập câu hỏi kìa.");
            }

            if (GeneralHelper.GetTotalWords(request.Question) > 30)
            {
                return Ok("Hỏi ngắn thôi cục cưng, bộ mắc hỏi quá hay gì 💢\nHỏi câu nào dưới 30 từ thôi, để thời gian cho mình suy nghĩ với chứ.");
            }

            try
            {
                var result = await ChatScope.GenerateAnswer(_accessKey, request, username, gender, age, englishLevel, enableReasoning, enableSearching);

                _logger.LogInformation($"{_accessKey[..10]} ({username}) asked (Reasoning: {enableReasoning} - Grounding: {enableSearching}): {request.Question}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot generate answer");

                // Check if it's a rate limit error (429)
                if (ex.Message.Contains("429") || ex.Message.Contains("RESOURCE_EXHAUSTED") || ex.Message.Contains("rate limit"))
                {
                    _logger.LogWarning("Rate limit hit for user: {Username}", username);
                    
                    return StatusCode(429, new ChatResponse
                    {
                        MessageInMarkdown = "🕐 **API đang quá tải**\n\nMình đang bị giới hạn số lượng yêu cầu rồi cục cưng. Vui lòng:\n\n1. ⏰ Đợi **1-2 phút** rồi thử lại\n2. 🔄 Hoặc **làm mới cuộc trò chuyện** (nút Làm mới)\n3. ⏳ Gửi tin nhắn **chậm hơn** một chút\n\nXin lỗi vì sự bất tiện này nha! 😊"
                    });
                }

                return Ok(new ChatResponse
                {
                    MessageInMarkdown = "Nhắn từ từ thôi cục cưng, vội vàng vậy 💢\nNgồi đợi 1 phút cho mình đi uống ly cà phê đã. Sau 1 phút mà vẫn lỗi thì xóa lịch sử trò chuyện rồi thử lại nha! ^_^"
                });
            }
        }
    }
}