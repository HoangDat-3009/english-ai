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
    }
}
