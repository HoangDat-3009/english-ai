using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Helper
{
    public static class HttpContextHelper
    {
        private static IHttpContextAccessor _accessor;
        private static IConfiguration _configuration;

        public static void Configure(IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _accessor = accessor;
            _configuration = configuration;
        }

        public static string? GetAccessKey()
        {
            // Read from configuration (appsettings.json)
            string key = _configuration["GeminiSettings:ApiKey"];

            return key;
        }

        public static string GetGeminiApiKey()
        {
            // Read from configuration (appsettings.json)
            string key = _configuration["Gemini:ApiKey"];

            return key;
        }

        public static string GetGpt5ApiKey()
        {
            // Try environment variable first, then fall back to configuration
            var envKey = Environment.GetEnvironmentVariable("GPT5_API_KEY");
            if (!string.IsNullOrWhiteSpace(envKey))
            {
                return envKey;
            }

            // Fall back to OpenAI API key from appsettings.json
            return _configuration?["OpenAI:ApiKey"] ?? string.Empty;
        }
    }
}
