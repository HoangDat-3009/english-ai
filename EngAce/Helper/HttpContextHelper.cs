using System;
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

        public static string? GetAccessKey() => Environment.GetEnvironmentVariable("ACCESS_KEY");

        public static string GetGeminiApiKey() => Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;

        public static string GetGpt5ApiKey()
        {
            return Environment.GetEnvironmentVariable("GPT5_API_KEY") ?? string.Empty;
        }
    }
}
