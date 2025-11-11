using Microsoft.AspNetCore.Http;

namespace Helper
{
    public static class HttpContextHelper
    {
        private static IHttpContextAccessor _accessor;

        public static void Configure(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public static string? GetAccessKey()
        {
            string key = "AIzaSyC9Eb96_EFvY17s2HRsrtCt56IDUa4lbzA";

            return key;
        }

        public static string GetGeminiApiKey()
        {
            // Trả về API key cho Gemini (có thể cùng key hoặc key riêng)
            string key = "AIzaSyC9Eb96_EFvY17s2HRsrtCt56IDUa4lbzA";
            return key;
        }
    }
}
