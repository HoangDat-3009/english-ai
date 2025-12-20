using EngAce.Api.Services.Auth;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EngAce.Api.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtService jwtService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                AttachUserToContext(context, jwtService, token);
            }

            await _next(context);
        }

        private void AttachUserToContext(HttpContext context, IJwtService jwtService, string token)
        {
            try
            {
                var principal = jwtService.ValidateToken(token);
                
                if (principal != null)
                {
                    context.User = principal;
                    
                    // Attach user ID to context items for easy access
                    var userIdClaim = principal.Claims.FirstOrDefault(x => x.Type == "sub");
                    if (userIdClaim != null)
                    {
                        context.Items["UserId"] = int.Parse(userIdClaim.Value);
                    }
                }
            }
            catch
            {
                // Token validation failed, do nothing
                // User will remain unauthenticated
            }
        }
    }
}
