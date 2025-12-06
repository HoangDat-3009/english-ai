using EngAce.Api.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EngAce.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<DebugController> _logger;

        public DebugController(IUserRepository userRepository, ILogger<DebugController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Debug endpoint to check user password hash
        /// </summary>
        [HttpGet("check-user/{emailOrUsername}")]
        public async Task<IActionResult> CheckUser(string emailOrUsername)
        {
            try
            {
                var user = emailOrUsername.Contains("@")
                    ? await _userRepository.GetByEmailAsync(emailOrUsername.ToLower().Trim())
                    : await _userRepository.GetByUsernameAsync(emailOrUsername.Trim());

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new
                {
                    userId = user.UserID,
                    username = user.Username,
                    email = user.Email,
                    hasPasswordHash = !string.IsNullOrEmpty(user.PasswordHash),
                    passwordHashLength = user.PasswordHash?.Length ?? 0,
                    passwordHashPreview = user.PasswordHash?.Length > 10 
                        ? user.PasswordHash.Substring(0, 10) + "..." 
                        : user.PasswordHash,
                    isOAuthUser = !string.IsNullOrEmpty(user.GoogleId) || !string.IsNullOrEmpty(user.FacebookId),
                    googleId = user.GoogleId,
                    facebookId = user.FacebookId,
                    status = user.Status,
                    createdAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Generate BCrypt hash for a password (for testing only)
        /// </summary>
        [HttpPost("generate-hash")]
        public IActionResult GenerateHash([FromBody] string password)
        {
            try
            {
                var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
                return Ok(new
                {
                    password = password,
                    hash = hash,
                    hashLength = hash.Length,
                    sqlCommand = $"UPDATE users SET password_hash = '{hash}' WHERE username = 'your_username';"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
