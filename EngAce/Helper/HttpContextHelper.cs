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
            string key = "AIzaSyA4ntpoBtLUwsppNOH7sXE9Dk4XuQ-maO8";

            return key;
        }
    }
}
