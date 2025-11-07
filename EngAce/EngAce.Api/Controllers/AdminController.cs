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
                var totalExercises = await _context.ReadingExercises.CountAsync();
                var totalQuestions = await _context.ReadingQuestions.CountAsync();
                var totalSubmissions = await _context.ReadingExerciseResults.CountAsync();
                
                var today = DateTime.Today;
                var thisWeek = today.AddDays(-(int)today.DayOfWeek);
                
                var activeUsersToday = await _context.ReadingExerciseResults
                    .Where(p => p.CompletedAt.Date == today)
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync();
                    
                var activeUsersThisWeek = await _context.ReadingExerciseResults
                    .Where(p => p.CompletedAt >= thisWeek)
                    .Select(p => p.UserId)
                    .Distinct()
                    .CountAsync();
                    
                var exercisesCreatedThisWeek = await _context.ReadingExercises
                    .Where(e => e.CreatedAt >= thisWeek)
                    .CountAsync();
                    
                var aiGeneratedExercises = await _context.ReadingExercises
                    .Where(e => e.SourceType.ToLower() == "ai")
                    .CountAsync();
                    
                var completedResults = await _context.ReadingExerciseResults
                    .Where(p => p.IsCompleted)
                    .ToListAsync();
                    
                var averageScore = completedResults.Any() 
                    ? (decimal)completedResults.Average(p => p.Score) 
                    : 0;
                    
                var averageCompletionTime = completedResults.Any()
                    ? completedResults.Average(p => (p.CompletedAt - p.StartedAt).TotalMinutes)
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
                
                // Top Users
                var topUsers = await _context.Users
                    .Select(u => new
                    {
                        User = u,
                        TotalExercises = u.ExerciseResults.Count(p => p.IsCompleted),
                        AverageScore = u.ExerciseResults.Where(p => p.IsCompleted).Average(p => (decimal?)p.Score) ?? 0,
                        TotalXP = u.TotalXP,
                        WeeklyXP = u.ExerciseResults.Where(p => p.CompletedAt >= thisWeek && p.IsCompleted).Sum(p => (int?)p.Score) ?? 0,
                        LastActivity = u.ExerciseResults.Where(p => p.IsCompleted).Max(p => (DateTime?)p.CompletedAt) ?? u.CreatedAt
                    })
                    .OrderByDescending(x => x.TotalXP)
                    .Take(10)
                    .ToListAsync();
                    
                dashboard.TopUsers = topUsers.Select(x => new TopUserDto
                {
                    UserId = x.User.Id.ToString(),
                    Username = x.User.Username,
                    FullName = x.User.FullName,
                    Email = x.User.Email,
                    TotalExercises = x.TotalExercises,
                    AverageScore = (double)x.AverageScore,
                    TotalXP = x.TotalXP,
                    WeeklyXP = (int)x.WeeklyXP,
                    LastActivity = x.LastActivity,
                    Status = x.User.IsActive ? "Active" : "Inactive"
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
                var exercise = await _context.ReadingExercises
                    .Include(e => e.Questions)
                    .Include(e => e.Results)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (exercise == null)
                {
                    return NotFound(new { message = "Exercise not found" });
                }

                var completedResults = exercise.Results.Where(p => p.IsCompleted).ToList();

                if (!completedResults.Any())
                {
                    return Ok(new ExerciseAnalyticsDto
                    {
                        ExerciseId = exercise.Id,
                        ExerciseName = exercise.Name,
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
                    .Where(p => p.CompletedAt >= thirtyDaysAgo)
                    .GroupBy(p => p.CompletedAt.Date)
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
                    ExerciseId = exercise.Id,
                    ExerciseName = exercise.Name,
                    TotalAttempts = completedResults.Count,
                    UniqueUsers = completedResults.Select(p => p.UserId).Distinct().Count(),
                    AverageScore = scores.Average(),
                    MinScore = scores.Min(),
                    MaxScore = scores.Max(),
                    AverageCompletionTime = TimeSpan.FromMinutes(
                        completedResults.Any()
                            ? completedResults.Average(p => (p.CompletedAt - p.StartedAt).TotalMinutes)
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
                var exercises = await _context.ReadingExercises
                    .Where(e => request.ExerciseIds.Contains(e.Id))
                    .ToListAsync();

                if (!exercises.Any())
                {
                    return BadRequest(new { message = "No exercises found with provided IDs" });
                }

                switch (request.Operation.ToLower())
                {
                    case "delete":
                        _context.ReadingExercises.RemoveRange(exercises);
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
    }
}