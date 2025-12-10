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
            // API key should be retrieved from configuration, not hardcoded
            // This is a placeholder - replace with actual configuration retrieval
            string key = "YOUR_API_KEY_HERE";

            return key;
        }
    }
}
