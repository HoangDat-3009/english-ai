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
            string key = "AIzaSyDGU_I8J6y29SZrW-7Fcx3PJ5cGEZoh808";

            return key;
        }

        public static string GetGeminiApiKey()
        {
            // Trả về API key cho Gemini (có thể cùng key hoặc key riêng)
            string key = "AIzaSyDGU_I8J6y29SZrW-7Fcx3PJ5cGEZoh808";
            return key;
        }
    }
}
