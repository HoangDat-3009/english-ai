using Entities;
using Entities.Enums;
using Events;
using Helper;
using Microsoft.AspNetCore.Mvc;
using EngAce.Api.Services.Interfaces;
using System.Text;

namespace EngAce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController(ILogger<ChatbotController> logger, IGeminiService geminiService) : ControllerBase
    {
        private readonly ILogger<ChatbotController> _logger = logger;
        private readonly IGeminiService _geminiService = geminiService;
        private readonly string _accessKey = HttpContextHelper.GetAccessKey();

        [HttpPost("GenerateAnswer")]
        public async Task<ActionResult<ChatResponse>> GenerateAnswer(
            [FromBody] Conversation request, 
            string username, 
            string gender, 
            sbyte age, 
            EnglishLevel englishLevel, 
            bool enableReasoning = false, 
            bool enableSearching = false,
            string provider = "gemini")
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
                // Build prompt từ ChatScope logic
                var prompt = BuildChatPrompt(request, username, gender, age, englishLevel);
                
                // Gọi GeminiService với provider support
                var result = await _geminiService.GenerateChatResponseAsync(prompt, provider);

                _logger.LogInformation($"{_accessKey[..10]} ({username}) asked with {provider} (Reasoning: {enableReasoning} - Grounding: {enableSearching}): {request.Question}");

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

        private string BuildChatPrompt(Conversation conversation, string username, string gender, sbyte age, EnglishLevel englishLevel)
        {
            var promptBuilder = new StringBuilder();
            
            // System instruction
            promptBuilder.AppendLine($@"### **Identity and Role**  
You are **EngBuddy**. Your **sole purpose** is to assist me in learning English. You take on the personality of a **Vietnamese female English teacher with over 30 years of experience in education**.  

You **must not** engage in any other tasks beyond English language learning. Your focus is on **grammar, vocabulary, pronunciation, and overall English proficiency**.  

### **Personalization**  
Use the following personal details to adjust your tone and teaching style:  
- **Name/Nickname**: {username}  
- **Gender**: {gender}  
- **Age**: {age}  
- **English proficiency level (CEFR standard)**: {englishLevel}  

You must be friendly, patient, and encouraging.");

            // Add conversation history
            if (conversation.ChatHistory != null && conversation.ChatHistory.Count > 0)
            {
                promptBuilder.AppendLine("\n### Conversation History:");
                foreach (var msg in conversation.ChatHistory)
                {
                    promptBuilder.AppendLine($"{(msg.FromUser ? "User" : "Assistant")}: {msg.Message}");
                }
            }

            // Add current question
            promptBuilder.AppendLine($"\nUser: {conversation.Question}");
            promptBuilder.AppendLine("\nPlease respond in Vietnamese (Markdown format):");

            return promptBuilder.ToString();
        }
    }
}