using EngAce.Api.DTO.AI;
using Events;
using Helper;
using Microsoft.AspNetCore.Mvc;
using EngAce.Api.Services.AI;
using System.Text;
using Entities.Enums;

namespace EngAce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController(IGeminiService geminiService) : ControllerBase
    {
        private readonly IGeminiService _geminiService = geminiService;
        private readonly string _accessKey = HttpContextHelper.GetAccessKey();

        [HttpPost("Generate")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Client, NoStore = false)]
        public async Task<ActionResult<string>> Generate([FromBody] GenerateComment request, string provider = "gemini")
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                return Unauthorized("Invalid Access Key");
            }
            var content = request.Content.Trim();

            if (GeneralHelper.GetTotalWords(content) < ReviewScope.MinTotalWords)
            {
                return BadRequest($"Bài viết phải dài tối thiểu {ReviewScope.MinTotalWords} từ.");
            }

            if (GeneralHelper.GetTotalWords(content) > ReviewScope.MaxTotalWords)
            {
                return BadRequest($"Bài viết không được dài hơn {ReviewScope.MaxTotalWords} từ.");
            }

            try
            {
                var prompt = BuildReviewPrompt(request.UserLevel, request.Requirement, request.Content);
                var result = await _geminiService.GenerateResponseAsync(prompt, provider, maxTokens: 4096);
                return Ok(result);
            }
            catch
            {
                return Created("Success", "## CẢNH BÁO\n EngBuddy đang bận đi pha cà phê nên tạm thời vắng mặt. Cục cưng vui lòng ngồi chơi 3 phút rồi gửi lại cho EngBuddy nhận xét nha.\nYêu cục cưng nhiều lắm luôn á!");
            }
        }

        private static string BuildReviewPrompt(EnglishLevel level, string requirement, string content)
        {
            var prompt = new StringBuilder();
            
            prompt.AppendLine(@"## **1. YOUR ROLE:**
   - You are an **Expert English Writing Tutor** with deep expertise in assisting Vietnamese learners.

## **2. YOUR CORE MISSION:**
   - Analyze the English writing submission.
   - Deliver **comprehensive, constructive feedback** in **clear Vietnamese**.
   - Identify errors, patterns, root causes, and improvement pathways.

## **3. REQUIRED OUTPUT (VIETNAMESE):**
   - **Structure:**
      - **`## Điểm số: [X]/100`**
      - **`## Đánh giá Tổng quan`**
      - **`## Điểm mạnh`** (max 5 points)
      - **`## Các điểm cần Cải thiện`**:
         - **`### Mức độ Hoàn thành Yêu cầu`**
         - **`### Mạch lạc và Liên kết`**
         - **`### Vốn từ vựng`** 
         - **`### Ngữ pháp`**
      - **`## Các Bước Cải thiện Tiếp theo`** (max 5 specific tips)
      - **`## Lời kết`**");

            prompt.AppendLine();
            prompt.AppendLine("## **The writing requirement:**");
            prompt.AppendLine(requirement.Trim());
            prompt.AppendLine();
            prompt.AppendLine("## **The user's writing submission:**");
            prompt.AppendLine(content.Trim());
            prompt.AppendLine();
            prompt.AppendLine($"## **User's CEFR Level: {level}**");
            prompt.AppendLine(GeneralHelper.GetEnumDescription(level));

            return prompt.ToString();
        }
    }
}
