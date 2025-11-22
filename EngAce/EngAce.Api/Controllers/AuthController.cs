using EngAce.Api.Helpers;
using Entities.Data;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<User>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Update last active time
            user.LastActiveAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var profileTier = UserProfileHelper.GetProfileTier(user.TotalXp);
            var studyStreak = await GetUserStudyStreakAsync(user.Id);

            // Return user without password
            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                fullName = user.FullName,
                profileTier,
                level = profileTier,
                studyStreak,
                totalStudyTime = user.TotalStudyTime,
                totalXp = user.TotalXp,
                accountType = user.AccountType,
                role = user.Role,
                createdAt = user.CreatedAt,
                lastActiveAt = user.LastActiveAt,
                avatar = user.Avatar,
                status = user.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.Username) || 
                string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new { message = "All fields are required" });
            }

            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser != null)
            {
                return Conflict(new { message = "Username or email already exists" });
            }

            // Validate email format
            if (!request.Email.Contains("@") || !request.Email.Contains("."))
            {
                return BadRequest(new { message = "Invalid email format" });
            }

            // Validate password length
            if (request.Password.Length < 6)
            {
                return BadRequest(new { message = "Password must be at least 6 characters" });
            }

            // Validate username length
            if (request.Username.Length < 4)
            {
                return BadRequest(new { message = "Username must be at least 4 characters" });
            }

            // Create new user
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password, // Plain text for now, can be hashed later
                FullName = request.FullName,
                TotalStudyTime = 0,
                TotalXp = 0,
                CreatedAt = DateTime.UtcNow,
                LastActiveAt = DateTime.UtcNow,
                Status = "active",
                AccountType = request.AccountType ?? "standard"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var profileTier = UserProfileHelper.GetProfileTier(user.TotalXp);

            // Return user without password
            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                fullName = user.FullName,
                profileTier,
                level = profileTier,
                studyStreak = 0,
                totalStudyTime = user.TotalStudyTime,
                totalXp = user.TotalXp,
                accountType = user.AccountType,
                role = user.Role,
                createdAt = user.CreatedAt,
                lastActiveAt = user.LastActiveAt,
                avatar = user.Avatar,
                status = user.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for username: {Username}", request.Username);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("update-profile")]
    public async Task<ActionResult<User>> UpdateProfile([FromBody] UpdateProfileRequest request, [FromQuery] int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Update user fields
            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName;
            
            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;
            
            if (!string.IsNullOrWhiteSpace(request.Password))
                user.Password = request.Password;

            if (!string.IsNullOrWhiteSpace(request.Avatar))
                user.AvatarUrl = request.Avatar;

            if (!string.IsNullOrWhiteSpace(request.AccountType))
                user.AccountType = request.AccountType;

            user.LastActiveAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var profileTier = UserProfileHelper.GetProfileTier(user.TotalXp);
            var studyStreak = await GetUserStudyStreakAsync(user.Id);

            // Return user without password
            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                fullName = user.FullName,
                profileTier,
                level = profileTier,
                studyStreak,
                totalStudyTime = user.TotalStudyTime,
                totalXp = user.TotalXp,
                accountType = user.AccountType,
                role = user.Role,
                createdAt = user.CreatedAt,
                lastActiveAt = user.LastActiveAt,
                avatar = user.Avatar,
                status = user.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user: {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AccountType { get; set; }
}

public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Avatar { get; set; }
    public string? AccountType { get; set; }
}

partial class AuthController
{
    private async Task<int> GetUserStudyStreakAsync(int userId)
    {
        var completionDates = await _context.Completions
            .Where(c => c.UserId == userId && c.CompletedAt.HasValue)
            .Select(c => c.CompletedAt!.Value)
            .ToListAsync();

        return UserProfileHelper.CalculateStudyStreak(completionDates);
    }
}

