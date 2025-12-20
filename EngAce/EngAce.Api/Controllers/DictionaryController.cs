using Entities;
using Events;
using Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using EngAce.Api.Services.AI;
using System.Text;

namespace EngAce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DictionaryController(IMemoryCache cache, ILogger<DictionaryController> logger, IGeminiService geminiService) : ControllerBase
    {
        private readonly IMemoryCache _cache = cache;
        private readonly ILogger<DictionaryController> _logger = logger;
        private readonly IGeminiService _geminiService = geminiService;
        private readonly string _accessKey = HttpContextHelper.GetAccessKey();

        [HttpGet("Search")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<ActionResult<SearchResult>> Search(string keyword, string? context, string provider = "gemini")
        {
            if (string.IsNullOrEmpty(_accessKey))
            {
                return Unauthorized("Invalid Access Key");
            }

            if (string.IsNullOrEmpty(keyword))
            {
                return BadRequest("Không được để trống từ khóa");
            }

            context = string.IsNullOrEmpty(context) ? "" : context.Trim();
            keyword = keyword.ToLower().Trim();

            var cacheKey = $"Search-{keyword}-{context.ToLower()}-{provider}";
            if (_cache.TryGetValue(cacheKey, out string cachedResult))
            {
                return Ok(cachedResult);
            }

            if (GeneralHelper.GetTotalWords(keyword) > SearchScope.MaxKeywordTotalWords)
            {
                return BadRequest($"Nội dung tra cứu chỉ chứa tối đa {SearchScope.MaxKeywordTotalWords} từ");
            }

            if (!GeneralHelper.IsEnglish(keyword))
            {
                return BadRequest("Từ khóa cần tra cứu phải là tiếng Anh");
            }

            if (!string.IsNullOrEmpty(context))
            {
                if (GeneralHelper.GetTotalWords(context) > SearchScope.MaxContextTotalWords)
                {
                    return BadRequest($"Ngữ cảnh chỉ chứa tối đa {SearchScope.MaxContextTotalWords} từ");
                }

                if (!GeneralHelper.IsEnglish(context))
                {
                    return BadRequest("Ngữ cảnh phải là tiếng Anh");
                }

                if (!context.Contains(keyword, StringComparison.CurrentCultureIgnoreCase))
                {
                    return BadRequest("Ngữ cảnh phải chứa từ khóa cần tra");
                }
            }

            try
            {
                var prompt = BuildDictionaryPrompt(keyword, context);
                var response = await _geminiService.GenerateDictionarySearchAsync(prompt, provider);
                
                _cache.Set(cacheKey, response, TimeSpan.FromDays(1));

                _logger.LogInformation("{_accessKey} searched with {provider}: {Keyword} - Context: {Context}", _accessKey[..10], provider, keyword, context);
                return Created("Success", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot search for the explaination of '{Keyword}' in the context '{Context}'", keyword, context);
                return Created("Success", "## CẢNH BÁO\n EngBuddy đang bận đi pha cà phê nên tạm thời vắng mặt. Bạn yêu vui lòng ngồi chơi 3 phút rồi tra lại thử nha.\nYêu bạn hiền nhiều lắm luôn á!");
            }
        }

        private string BuildDictionaryPrompt(string keyword, string? context)
        {
            var prompt = new StringBuilder();
            
            prompt.AppendLine(@"Bạn là một từ điển Anh-Việt toàn diện, chính xác và giàu tính ứng dụng, được thiết kế để giúp người dùng hiểu và sử dụng từ vựng một cách tự nhiên, đúng ngữ pháp và phù hợp với ngữ cảnh.");
            prompt.AppendLine(@"
##  Mục Tiêu Chính  
1. Giải nghĩa chính xác & dễ hiểu, ưu tiên nghĩa phù hợp nhất với ngữ cảnh.  
2. Hướng dẫn cách sử dụng từ đúng văn phong & ngữ pháp.  
3. Liệt kê lỗi sai phổ biến & cách tránh.  
4. Cung cấp thông tin thú vị, mẹo ghi nhớ & nguồn gốc từ vựng.  
5. Tổng hợp từ đồng nghĩa, trái nghĩa & cụm từ liên quan.  
6. Lập bảng so sánh với các từ/cụm từ tương tự nếu cần.

#  Lưu Ý: Luôn viết bằng tiếng Việt, trình bày khoa học, dễ đọc.

#  CẤU TRÚC PHẢN HỒI: 
1. PHÁT ÂM (IPA, trọng âm)
2. GIẢI NGHĨA (nghĩa phổ biến + ví dụ)  
3. CÁCH DÙNG (cấu trúc câu, collocation, văn phong)
4. LỖI SAI PHỔ BIẾN
5. TỪ ĐỒNG NGHĨA & TRÁI NGHĨA (có bảng so sánh nếu cần)
6. THÔNG TIN THÚ VỊ & MẸO GHI NHỚ
7. TỔNG KẾT");

            prompt.AppendLine($"\nTừ khóa: {keyword}");
            
            if (!string.IsNullOrEmpty(context))
            {
                prompt.AppendLine($"Ngữ cảnh: {context}");
                prompt.AppendLine("\nƯu tiên nghĩa phù hợp với ngữ cảnh này.");
            }

            return prompt.ToString();
        }
    }
}