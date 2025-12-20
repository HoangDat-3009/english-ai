using EngAce.Api.DTO.Auth;
using EngAce.Api.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EngAce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user with email and password
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("📝 Registration attempt for email: {Email}", request?.Email);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("❌ Invalid model state for registration");
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid input data",
                    });
                }

                var result = await _authService.RegisterAsync(request);

                if (!result.Success)
                {
                    _logger.LogWarning("❌ Registration failed: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("✅ Registration successful for email: {Email}", request?.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Exception during registration for email: {Email}", request?.Email);
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = $"An error occurred during registration: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Login with email/username and password
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid input data"
                });
            }

            var result = await _authService.LoginAsync(request);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Login or register via OAuth (Google/Facebook)
        /// </summary>
        [HttpPost("oauth-login")]
        public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Message = "Invalid input data"
                });
            }

            var result = await _authService.OAuthLoginAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Get user ID from JWT token
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { success = false, message = "Invalid or missing token" });
            }

            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            return Ok(new { success = true, user = user });
        }

        /// <summary>
        /// Check if server is running
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
    }
}
