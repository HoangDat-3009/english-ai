using Microsoft.AspNetCore.Http;

namespace Helper
{
    public static class HttpContextHelper
    {
        private static IHttpContextAccessor _accessor;
        private static GeminiKeyManager? _keyManager;

        public static void Configure(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public static void ConfigureKeyManager(GeminiKeyManager keyManager)
        {
            _keyManager = keyManager;
        }

        public static string? GetAccessKey()
        {
            if (_keyManager != null)
            {
                return _keyManager.GetCurrentKey();
            }

            // Fallback nếu chưa configure key manager
            return "AIzaSyA4ntpoBtLUwsppNOH7sXE9Dk4XuQ-maO8";
        }

        public static GeminiKeyManager? GetKeyManager()
        {
            return _keyManager;
        }
    }
}
