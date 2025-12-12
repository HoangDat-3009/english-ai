using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using EngAce.Api.DTO.Admin;
using EngAce.Api.DTO.Core;
using EngAce.Api.DTO.Shared;
using Entities.Data;
using Entities.Models;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace EngAce.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AdminController> _logger;
        private readonly ApplicationDbContext _context;

        public AdminController(IWebHostEnvironment env, ILogger<AdminController> logger, ApplicationDbContext context)
        {
            _env = env;
            _logger = logger;
            _context = context;
        }

        // ===== DASHBOARD =====
        
        /// <summary>
        /// Get comprehensive admin dashboard data
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult> GetDashboard()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                var dashboard = new AdminDashboardDto();
                
                // System Statistics
                var stats = new SystemStatistics();
                
                var totalUsers = await _context.Users.CountAsync();
                var totalExercises = await _context.Exercises.CountAsync(); // Changed from ReadingExercises
                
                // Load exercises to client-side to count questions (EF Core can't translate custom methods to SQL)
                var exercisesForCounting = await _context.Exercises
                    .Select(e => e.Questions)
                    .ToListAsync();
                var totalQuestions = exercisesForCounting.Sum(q => CountQuestionsFromJson(q));
                
                var totalSubmissions = await _context.Completions.CountAsync(); // Changed from ReadingExerciseResults
                
                var today = DateTime.Today;
                var thisWeek = today.AddDays(-(int)today.DayOfWeek);
                
                var activeUsersToday = await _context.Completions // Changed from ReadingExerciseResults
                    .Where(p => p.CompletedAt.HasValue && p.CompletedAt.Value.Date == today) // Handle nullable CompletedAt
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync();
                    
                var activeUsersThisWeek = await _context.Completions // Changed from ReadingExerciseResults
                    .Where(p => p.CompletedAt.HasValue && p.CompletedAt.Value >= thisWeek) // Handle nullable CompletedAt
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync();
                    
                var exercisesCreatedThisWeek = await _context.Exercises // Changed from ReadingExercises
                    .Where(e => e.CreatedAt >= thisWeek)
                    .CountAsync();
                    
                var aiGeneratedExercises = await _context.Exercises // Changed from ReadingExercises
                    .Where(e => e.Type.ToLower().Contains("ai") || e.Category.ToLower().Contains("ai")) // Adjusted logic
                    .CountAsync();
                    
                var completedResults = await _context.Completions // Changed from ReadingExerciseResults
                    .Where(p => p.IsCompleted)
                    .ToListAsync();
                    
                var averageScore = completedResults.Any() 
                    ? (decimal)completedResults.Average(p => p.Score) 
                    : 0;
                    
                var averageCompletionTime = completedResults.Any()
                    ? completedResults.Where(p => p.CompletedAt.HasValue && p.StartedAt.HasValue)
                        .Average(p => (p.CompletedAt!.Value - p.StartedAt!.Value).TotalMinutes)
                    : 0;
                    
                stats = new SystemStatistics
                {
                    TotalUsers = totalUsers,
                    TotalExercises = totalExercises,
                    TotalQuestions = totalQuestions,
                    TotalSubmissions = totalSubmissions,
                    ActiveUsersToday = activeUsersToday,
                    ActiveUsersThisWeek = activeUsersThisWeek,
                    ExercisesCreatedThisWeek = exercisesCreatedThisWeek,
                    AIGeneratedExercises = aiGeneratedExercises,
                    AverageScore = (double)averageScore,
                    AverageCompletionTime = (double)averageCompletionTime
                };
                
                dashboard.Statistics = stats;
                
                // Recent Activities - skip for now due to JSON conflicts
                
                // Top Users - Load separately and join client-side (same approach as ProgressService)
                var topUsersList = await _context.Users
                    .Where(u => u.Status == "active" || u.Status == null)
                    .OrderByDescending(u => u.TotalXp)
                    .Take(10)
                    .ToListAsync();
                
                var topUserIds = topUsersList.Select(u => u.Id).ToList();
                var userCompletions = await _context.Completions
                    .Where(c => topUserIds.Contains(c.UserId) && c.IsCompleted)
                    .ToListAsync();
                    
                dashboard.TopUsers = topUsersList.Select(user =>
                {
                    var completions = userCompletions.Where(c => c.UserId == user.Id).ToList();
                    var completedCount = completions.Count;
                    var avgScore = completions.Any() ? completions.Average(c => c.Score) : 0;
                    var weeklyXP = completions
                        .Where(c => c.CompletedAt.HasValue && c.CompletedAt.Value >= thisWeek)
                        .Sum(c => (int)c.Score);
                    var lastActivity = completions
                        .Where(c => c.CompletedAt.HasValue)
                        .Max(c => (DateTime?)c.CompletedAt) ?? user.CreatedAt;
                    
                    return new TopUserDto
                    {
                        UserId = user.Id.ToString(),
                        Username = user.Username,
                        FullName = user.FullName,
                        Email = user.Email,
                        TotalExercises = completedCount,
                        AverageScore = (double)avgScore,
                        TotalXP = user.TotalXp,
                        WeeklyXP = weeklyXP,
                        LastActivity = lastActivity,
                        Status = user.Status == "active" ? "Active" : "Inactive"
                    };
                }).ToList();
                
                // System Health
                stopwatch.Stop();
                var systemHealth = new SystemHealthDto
                {
                    DatabaseConnection = true, // If we get here, DB is connected
                    GeminiApiConnection = true, // TODO: Add Gemini health check
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    CpuUsagePercent = 0, // TODO: Add system metrics
                    MemoryUsagePercent = 0,
                    ApplicationVersion = "1.0.0",
                    LastCheckTime = DateTime.UtcNow
                };
                
                dashboard.SystemHealth = systemHealth;
                
                // Trả về basic object để tránh ActivityDto JSON conflicts
                return Ok(new {
                    statistics = stats,
                    topUsers = dashboard.TopUsers,
                    systemHealth = dashboard.SystemHealth,
                    message = "Dashboard data (Activities temporarily disabled due to JSON serialization issues)"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard");
                return StatusCode(500, new { message = "Error loading dashboard", error = ex.Message });
            }
        }

        // ===== USER MANAGEMENT MOVED TO UserManagementController =====
        // All user management endpoints have been moved to /api/user-management
        // This controller now focuses only on admin dashboard and system overview

        // ===== EXERCISE MANAGEMENT =====
        // Note: Basic exercise CRUD has been moved to ReadingExerciseController
        // This controller now focuses on admin-specific analytics and bulk operations

        /// <summary>
        /// Get detailed analytics for a specific exercise
        /// </summary>
        [HttpGet("exercises/{id}/analytics")]
        public async Task<ActionResult<ExerciseAnalyticsDto>> GetExerciseAnalytics(int id)
        {
            try
            {
                var exercise = await _context.Exercises // Changed from ReadingExercises
                    .Include(e => e.Completions) // Changed from Results
                    .FirstOrDefaultAsync(e => e.ExerciseId == id); // Changed from Id to ExerciseId

                if (exercise == null)
                {
                    return NotFound(new { message = "Exercise not found" });
                }

                var completedResults = exercise.Completions.Where(p => p.IsCompleted).ToList(); // Changed from Results

                if (!completedResults.Any())
                {
                    return Ok(new ExerciseAnalyticsDto
                    {
                        ExerciseId = exercise.ExerciseId, // Changed from Id
                        ExerciseName = exercise.Title, // Changed from Name
                        TotalAttempts = 0,
                        UniqueUsers = 0,
                        AverageScore = 0,
                        MinScore = 0,
                        MaxScore = 0,
                        AverageCompletionTime = TimeSpan.Zero,
                        ScoreDistribution = new List<ScoreDistribution>(),
                        DailyAttempts = new List<DailyAttempts>(),
                        QuestionAnalytics = new List<QuestionAnalytics>()
                    });
                }

                var scores = completedResults.Select(p => (decimal)p.Score).ToList();

                // Score distribution
                var scoreDistribution = new List<ScoreDistribution>
                {
                    new() { ScoreRange = "0-20", Count = scores.Count(s => s <= 20), Percentage = 0 },
                    new() { ScoreRange = "21-40", Count = scores.Count(s => s > 20 && s <= 40), Percentage = 0 },
                    new() { ScoreRange = "41-60", Count = scores.Count(s => s > 40 && s <= 60), Percentage = 0 },
                    new() { ScoreRange = "61-80", Count = scores.Count(s => s > 60 && s <= 80), Percentage = 0 },
                    new() { ScoreRange = "81-100", Count = scores.Count(s => s > 80), Percentage = 0 }
                };

                foreach (var dist in scoreDistribution)
                {
                    dist.Percentage = scores.Count > 0 ? (decimal)dist.Count / scores.Count * 100 : 0;
                }

                // Daily attempts (last 30 days)
                var thirtyDaysAgo = DateTime.Today.AddDays(-30);
                var dailyAttempts = completedResults
                    .Where(p => p.CompletedAt.HasValue && p.CompletedAt.Value >= thirtyDaysAgo) // Handle nullable CompletedAt
                    .GroupBy(p => p.CompletedAt.Value.Date) // Safe to use .Value after null check
                    .Select(g => new DailyAttempts
                    {
                        Date = g.Key,
                        AttemptCount = g.Count(),
                        UniqueUsers = g.Select(p => p.UserId).Distinct().Count(),
                        AverageScore = (decimal)g.Average(p => p.Score)
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                var analytics = new ExerciseAnalyticsDto
                {
                    ExerciseId = exercise.ExerciseId, // Changed from Id
                    ExerciseName = exercise.Title, // Changed from Name
                    TotalAttempts = completedResults.Count,
                    UniqueUsers = completedResults.Select(p => p.UserId).Distinct().Count(),
                    AverageScore = scores.Average(),
                    MinScore = scores.Min(),
                    MaxScore = scores.Max(),
                    AverageCompletionTime = TimeSpan.FromMinutes(
                        completedResults.Any()
                            ? completedResults.Where(p => p.CompletedAt.HasValue && p.StartedAt.HasValue)
                                .Average(p => (p.CompletedAt!.Value - p.StartedAt!.Value).TotalMinutes)
                            : 0
                    ),
                    ScoreDistribution = scoreDistribution,
                    DailyAttempts = dailyAttempts,
                    QuestionAnalytics = new List<QuestionAnalytics>() // TODO: Implement question-level analytics
                };

                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exercise analytics for exercise {ExerciseId}", id);
                return StatusCode(500, new { message = "Error loading analytics", error = ex.Message });
            }
        }

        /// <summary>
        /// Bulk operations on exercises
        /// </summary>
        [HttpPost("exercises/bulk")]
        public async Task<IActionResult> BulkOperationExercises([FromBody] BulkOperationRequest request)
        {
            try
            {
                var exercises = await _context.Exercises // Changed from ReadingExercises
                    .Where(e => request.ExerciseIds.Contains(e.ExerciseId)) // Changed from Id to ExerciseId
                    .ToListAsync();

                if (!exercises.Any())
                {
                    return BadRequest(new { message = "No exercises found with provided IDs" });
                }

                switch (request.Operation.ToLower())
                {
                    case "delete":
                        _context.Exercises.RemoveRange(exercises); // Changed from ReadingExercises
                        break;
                        
                    case "activate":
                        foreach (var exercise in exercises)
                            exercise.IsActive = true;
                        break;
                        
                    case "deactivate":
                        foreach (var exercise in exercises)
                            exercise.IsActive = false;
                        break;
                        
                    default:
                        return BadRequest(new { message = "Invalid operation" });
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = $"Successfully performed {request.Operation} on {exercises.Count} exercises" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk operation {Operation}", request.Operation);
                return StatusCode(500, new { message = "Error performing bulk operation", error = ex.Message });
            }
        }

        // Exercise deletion moved to ReadingExerciseController: DELETE /api/ReadingExercise/{id}

        // ===== FILE UPLOAD MOVED TO ReadingExerciseController =====
        // File upload functionality is handled by ReadingExerciseController
        // POST /api/ReadingExercise/upload - for exercise file uploads
        // This keeps admin controller focused on dashboard and analytics only

        // Helper method to count questions from JSON
        private static int CountQuestionsFromJson(string questionsJson)
        {
            try
            {
                var questions = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement[]>(questionsJson);
                return questions?.Length ?? 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}