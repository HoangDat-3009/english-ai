using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EngAce.Api.DTO.Core;
using EngAce.Api.DTO.Admin;
using EngAce.Api.DTO.Exercises;
using Entities.Data;
using Entities.Models;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace EngAce.Api.Controllers
{
    [ApiController]
    [Route("api/user-management")]
    public class UserManagementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(ApplicationDbContext context, ILogger<UserManagementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ===== USER CRUD OPERATIONS =====
        
        /// <summary>
        /// Get paginated list of users with filtering (Di chuyển từ AdminController)
        /// </summary>
        [HttpGet("users")]
        public async Task<ActionResult<PagedResult<UserManagementDto>>> GetUsers(
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? level = null,
            [FromQuery] string orderBy = "CreatedAt",
            [FromQuery] bool orderDesc = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Users.AsQueryable();
                
                // Apply filters
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.Username.Contains(search) || 
                                           u.FullName.Contains(search) ||
                                           u.Email.Contains(search));
                }
                
                if (!string.IsNullOrEmpty(status))
                {
                    var isActive = status.ToLower() == "active";
                    query = query.Where(u => u.IsActive == isActive);
                }
                
                if (!string.IsNullOrEmpty(level))
                {
                    query = query.Where(u => u.Level == level);
                }
                
                // Apply ordering
                query = orderBy.ToLower() switch
                {
                    "username" => orderDesc ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
                    "fullname" => orderDesc ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
                    "email" => orderDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                    "totalxp" => orderDesc ? query.OrderByDescending(u => u.TotalXP) : query.OrderBy(u => u.TotalXP),
                    _ => orderDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
                };
                
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                
                var users = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(u => u.ExerciseResults)
                    .Select(u => new
                    {
                        User = u,
                        TotalExercises = u.ExerciseResults.Count(p => p.IsCompleted),
                        AverageScore = u.ExerciseResults.Where(p => p.IsCompleted).Average(p => (decimal?)p.Score) ?? 0,
                        WeeklyXP = u.ExerciseResults
                            .Where(p => p.CompletedAt >= DateTime.Today.AddDays(-7) && p.IsCompleted)
                            .Sum(p => (int?)p.Score) ?? 0,
                        MonthlyXP = u.ExerciseResults
                            .Where(p => p.CompletedAt >= DateTime.Today.AddDays(-30) && p.IsCompleted)
                            .Sum(p => (int?)p.Score) ?? 0
                    })
                    .ToListAsync();
                    
                var userDtos = users.Select(x => new UserManagementDto
                {
                    Id = x.User.Id.ToString(),
                    Username = x.User.Username,
                    FullName = x.User.FullName,
                    Email = x.User.Email,
                    CreatedAt = x.User.CreatedAt,
                    LastLoginAt = x.User.LastActiveAt,
                    Status = x.User.IsActive ? "Active" : "Inactive",
                    Level = CalculateUserLevel(x.User.TotalXP).ToString(), // Tính level từ XP
                    TotalXP = x.User.TotalXP,
                    WeeklyXP = (int)x.WeeklyXP,
                    MonthlyXP = (int)x.MonthlyXP,
                    TotalExercisesCompleted = x.TotalExercises,
                    AverageScore = (double)x.AverageScore,
                    StreakDays = x.User.StudyStreak,
                    PreferredLevel = x.User.Level,
                    Achievements = CalculateAchievements(x.User, x.TotalExercises, x.User.StudyStreak)
                }).ToList();
                
                var result = new PagedResult<UserManagementDto>
                {
                    Data = userDtos,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new { message = "Error loading users", error = ex.Message });
            }
        }

        /// <summary>
        /// Get specific user details với đầy đủ progress info
        /// </summary>
        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserDetailDto>> GetUserDetail(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.ExerciseResults)
                        .ThenInclude(r => r.ReadingExercise)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var completedExercises = user.ExerciseResults.Where(p => p.IsCompleted).ToList();
                var todayResults = completedExercises.Where(r => r.CompletedAt.Date == DateTime.Today).ToList();
                var weekResults = completedExercises.Where(r => r.CompletedAt >= DateTime.Today.AddDays(-7)).ToList();
                var monthResults = completedExercises.Where(r => r.CompletedAt >= DateTime.Today.AddDays(-30)).ToList();
                
                var userDetail = new UserDetailDto
                {
                    // Basic Info
                    Id = user.Id.ToString(),
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastActiveAt,
                    Status = user.IsActive ? "Active" : "Inactive",
                    
                    // Level & XP
                    Level = CalculateUserLevel(user.TotalXP).ToString(),
                    TotalXP = user.TotalXP,
                    WeeklyXP = weekResults.Sum(r => r.Score),
                    MonthlyXP = monthResults.Sum(r => r.Score),
                    TodayXP = todayResults.Sum(r => r.Score),
                    
                    // Exercise Stats
                    TotalExercisesCompleted = completedExercises.Count,
                    AverageScore = completedExercises.Any() ? (double)completedExercises.Average(p => p.Score) : 0,
                    BestScore = completedExercises.Any() ? completedExercises.Max(p => p.Score) : 0,
                    
                    // Streak & Activity
                    StreakDays = user.StudyStreak,
                    PreferredLevel = user.Level,
                    
                    // Achievements
                    Achievements = CalculateAchievements(user, completedExercises.Count, user.StudyStreak),
                    
                    // Recent Activities
                                        RecentExercises = completedExercises
                        .OrderByDescending(r => r.CompletedAt)
                        .Take(10)
                        .Select(r => new RecentExerciseDto
                        {
                            ExerciseId = r.ReadingExercise.Id,
                            Title = r.ReadingExercise.Name ?? "",
                            Type = r.ReadingExercise.Type,
                            Score = r.Score,
                            CompletedAt = r.CompletedAt,
                            Duration = r.CompletedAt - r.StartedAt
                        }).ToList().Cast<object>().ToList(),
                        
                    // Performance by exercise type
                    PerformanceByType = completedExercises
                        .GroupBy(r => r.ReadingExercise.Type)
                        .Select(g => new ExerciseTypePerformanceDto
                        {
                            Type = g.Key,
                            TotalAttempts = g.Count(),
                            AverageScore = (double)g.Average(r => r.Score),
                            BestScore = g.Max(r => r.Score),
                            TotalTimeSpent = TimeSpan.FromMinutes(g.Sum(r => (r.CompletedAt - r.StartedAt).TotalMinutes))
                        }).ToList().Cast<object>().ToList()
                };

                return Ok(userDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user detail {UserId}", id);
                return StatusCode(500, new { message = "Error loading user detail", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new user
        /// </summary>
        [HttpPost("users")]
        public async Task<ActionResult<UserManagementDto>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == request.UserName))
                {
                    return BadRequest(new { message = "Username already exists" });
                }

                var user = new User
                {
                    Username = request.UserName,
                    FullName = request.FullName ?? request.UserName,
                    Email = request.Email ?? "",
                    Level = request.PreferredLevel ?? "Beginner",
                    CreatedAt = DateTime.UtcNow,
                    LastActiveAt = DateTime.UtcNow,
                    IsActive = request.Status == "Active",
                    TotalXP = 0,
                    StudyStreak = 0
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userDto = new UserManagementDto
                {
                    Id = user.Id.ToString(),
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    Status = request.Status ?? "Active",
                    Level = "1",
                    TotalXP = 0,
                    WeeklyXP = 0,
                    MonthlyXP = 0,
                    TotalExercisesCompleted = 0,
                    AverageScore = 0,
                    StreakDays = 0,
                    PreferredLevel = user.Level,
                    Achievements = new List<string>()
                };

                return CreatedAtAction(nameof(GetUserDetail), new { id = user.Id }, userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { message = "Error creating user", error = ex.Message });
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        [HttpPut("users/{id}")]
        public async Task<ActionResult<UserManagementDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.FullName))
                    user.FullName = request.FullName;
                    
                if (!string.IsNullOrEmpty(request.Email))
                    user.Email = request.Email;
                    
                if (!string.IsNullOrEmpty(request.PreferredLevel))
                    user.Level = request.PreferredLevel;
                    
                if (!string.IsNullOrEmpty(request.Status))
                    user.IsActive = request.Status == "Active";
                    
                if (request.TotalXP > 0)
                    user.TotalXP = request.TotalXP;

                await _context.SaveChangesAsync();

                // Return updated user data
                var updatedUser = await GetUserDetail(id);
                return Ok(updatedUser.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new { message = "Error updating user", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete user (soft delete)
        /// </summary>
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Soft delete - mark as inactive
                user.IsActive = false;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new { message = "Error deleting user", error = ex.Message });
            }
        }

        // ===== LEADERBOARD ENDPOINTS =====
        
        // Leaderboard functionality moved to LeaderboardController: GET /api/Leaderboard
        // This avoids duplicate endpoints and keeps user management focused on CRUD operations

        // ===== EXCEL IMPORT/EXPORT =====
        
        /// <summary>
        /// Import users từ Excel file
        /// </summary>
        [HttpPost("import-excel")]
        public async Task<ActionResult<ImportResultDto>> ImportUsersFromExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "No file uploaded" });
                }

                if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                {
                    return BadRequest(new { message = "Invalid file format. Please upload Excel file." });
                }

                var importResult = new ImportResultDto
                {
                    TotalRows = 0,
                    SuccessCount = 0,
                    ErrorCount = 0,
                    Errors = new List<string>()
                };

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                importResult.TotalRows = rowCount - 1; // Excluding header

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var username = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var fullName = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        var email = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                        var level = worksheet.Cells[row, 4].Value?.ToString()?.Trim() ?? "Beginner";

                        if (string.IsNullOrEmpty(username))
                        {
                            importResult.Errors.Add($"Row {row}: Username is required");
                            importResult.ErrorCount++;
                            continue;
                        }

                        // Check if user already exists
                        if (await _context.Users.AnyAsync(u => u.Username == username))
                        {
                            importResult.Errors.Add($"Row {row}: Username '{username}' already exists");
                            importResult.ErrorCount++;
                            continue;
                        }

                        var user = new User
                        {
                            Username = username,
                            FullName = fullName ?? username,
                            Email = email ?? "",
                            Level = level,
                            CreatedAt = DateTime.UtcNow,
                            LastActiveAt = DateTime.UtcNow,
                            IsActive = true,
                            TotalXP = 0,
                            StudyStreak = 0
                        };

                        _context.Users.Add(user);
                        importResult.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        importResult.Errors.Add($"Row {row}: {ex.Message}");
                        importResult.ErrorCount++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(importResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing users from Excel");
                return StatusCode(500, new { message = "Error importing users", error = ex.Message });
            }
        }

        /// <summary>
        /// Export users to Excel
        /// </summary>
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportUsersToExcel()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.ExerciseResults)
                    .OrderByDescending(u => u.TotalXP)
                    .ToListAsync();

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Users");

                // Headers
                worksheet.Cells[1, 1].Value = "Username";
                worksheet.Cells[1, 2].Value = "Full Name";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Level";
                worksheet.Cells[1, 5].Value = "Total XP";
                worksheet.Cells[1, 6].Value = "Exercises Completed";
                worksheet.Cells[1, 7].Value = "Average Score";
                worksheet.Cells[1, 8].Value = "Streak Days";
                worksheet.Cells[1, 9].Value = "Status";
                worksheet.Cells[1, 10].Value = "Created At";
                worksheet.Cells[1, 11].Value = "Last Active";

                // Data
                for (int i = 0; i < users.Count; i++)
                {
                    var user = users[i];
                    var row = i + 2;
                    var completedExercises = user.ExerciseResults.Where(r => r.IsCompleted).ToList();

                    worksheet.Cells[row, 1].Value = user.Username;
                    worksheet.Cells[row, 2].Value = user.FullName;
                    worksheet.Cells[row, 3].Value = user.Email;
                    worksheet.Cells[row, 4].Value = user.Level;
                    worksheet.Cells[row, 5].Value = user.TotalXP;
                    worksheet.Cells[row, 6].Value = completedExercises.Count;
                    worksheet.Cells[row, 7].Value = completedExercises.Any() ? completedExercises.Average(r => r.Score) : 0;
                    worksheet.Cells[row, 8].Value = user.StudyStreak;
                    worksheet.Cells[row, 9].Value = user.IsActive ? "Active" : "Inactive";
                    worksheet.Cells[row, 10].Value = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                    worksheet.Cells[row, 11].Value = user.LastActiveAt.ToString("yyyy-MM-dd HH:mm:ss");
                }

                worksheet.Cells.AutoFitColumns();

                var fileName = $"Users_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                var fileBytes = package.GetAsByteArray();

                return File(fileBytes, 
                           "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                           fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting users to Excel");
                return StatusCode(500, new { message = "Error exporting users", error = ex.Message });
            }
        }

        // ===== HELPER METHODS =====

        private int CalculateUserLevel(int totalXP)
        {
            // XP thresholds cho từng level
            return totalXP switch
            {
                < 100 => 1,
                < 300 => 2,
                < 600 => 3,
                < 1000 => 4,
                < 1500 => 5,
                < 2100 => 6,
                < 2800 => 7,
                < 3600 => 8,
                < 4500 => 9,
                < 5500 => 10,
                _ => Math.Min(10 + (totalXP - 5500) / 1000, 50) // Max level 50
            };
        }

        private List<string> CalculateAchievements(User user, int totalExercises, int streakDays)
        {
            var achievements = new List<string>();

            // XP-based achievements
            if (user.TotalXP >= 100) achievements.Add("First Steps");
            if (user.TotalXP >= 1000) achievements.Add("XP Collector");
            if (user.TotalXP >= 5000) achievements.Add("XP Master");

            // Exercise-based achievements
            if (totalExercises >= 1) achievements.Add("Getting Started");
            if (totalExercises >= 10) achievements.Add("Practice Makes Perfect");
            if (totalExercises >= 50) achievements.Add("Exercise Veteran");
            if (totalExercises >= 100) achievements.Add("TOEIC Expert");

            // Streak-based achievements
            if (streakDays >= 3) achievements.Add("Consistent Learner");
            if (streakDays >= 7) achievements.Add("Week Warrior");
            if (streakDays >= 30) achievements.Add("Month Master");
            if (streakDays >= 100) achievements.Add("Streak Legend");

            return achievements;
        }

        private string GetUserBadge(int rank, int streakDays, int totalExercises)
        {
            return rank switch
            {
                1 => "🏆", // Gold
                2 => "🥈", // Silver  
                3 => "🥉", // Bronze
                _ when streakDays >= 30 => "🔥", // Fire streak
                _ when totalExercises >= 100 => "⭐", // Star performer
                _ when rank <= 10 => "💎", // Diamond top 10
                _ => "🚀" // Default rocket
            };
        }
    }
}