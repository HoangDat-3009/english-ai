using EngAce.Api.DTO.Core;
using EngAce.Api.DTO.Shared;
using EngAce.Api.DTO.Admin;
using EngAce.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EngAce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;
    private readonly ILogger<ProgressController> _logger;

    public ProgressController(IProgressService progressService, ILogger<ProgressController> logger)
    {
        _progressService = progressService;
        _logger = logger;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<UserProgressDto>> GetUserProgress(int userId)
    {
        try
        {
            var progress = await _progressService.GetUserProgressAsync(userId);
            if (progress == null)
                return NotFound(new { message = $"User progress for ID {userId} not found" });

            // Format response to match frontend UserProgress interface
            var response = new
            {
                userId = progress.UserId,
                username = progress.Username,
                fullName = progress.FullName,
                email = progress.Email,
                level = progress.Level,
                totalScore = progress.TotalScore,
                listening = progress.Listening,
                speaking = progress.Speaking,
                reading = progress.Reading,
                writing = progress.Writing,
                studyStreak = progress.StudyStreak,
                totalStudyTime = (int)progress.TotalStudyTime.TotalMinutes, // Convert to minutes
                totalXP = progress.TotalXP,
                achievements = progress.Achievements ?? new List<string>(),
                lastActive = progress.LastActive.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                completedExercises = progress.CompletedExercises,
                totalExercisesAvailable = progress.TotalExercisesAvailable,
                averageAccuracy = progress.AverageAccuracy,
                createdAt = progress.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                updatedAt = progress.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                toeicParts = progress.ToeicParts ?? new List<ToeicPartScoreDto>() // ✅ Add toeicParts to response
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user progress for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("weekly/{userId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetWeeklyProgress(int userId)
    {
        try
        {
            var weeklyProgress = await _progressService.GetWeeklyProgressAsync(userId);
            
            // Format response to match frontend WeeklyProgress[] interface
            var response = weeklyProgress.DailyProgress.Select(dp => new
            {
                day = dp.Day,
                exercises = dp.Exercises,
                time = (int)dp.Time.TotalMinutes, // Convert to minutes
                date = dp.Date.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                exercisesCompleted = dp.ExercisesCompleted,
                timeSpentMinutes = dp.TimeSpentMinutes,
                xpEarned = dp.XPEarned
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weekly progress for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("activities/{userId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetUserActivities(int userId, [FromQuery] int limit = 10)
    {
        try
        {
            var activities = await _progressService.GetUserActivitiesAsync(userId, limit);
            
            // Format response to match frontend Activity[] interface
            var response = activities.Select(a => new
            {
                id = int.TryParse(a.Id, out var id) ? id : 0,
                type = a.Type,
                topic = a.Topic,
                date = a.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                score = a.Score ?? 0,
                duration = (int)a.Duration.TotalMinutes, // Convert to minutes
                assignmentType = a.AssignmentType,
                timeSpentMinutes = a.TimeSpentMinutes,
                xpEarned = a.XPEarned,
                status = a.Status
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activities for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("user/{userId}")]
    public async Task<ActionResult<UserProgressDto>> UpdateUserProgress(int userId, [FromBody] UpdateProgressDto updateDto)
    {
        try
        {
            var progress = await _progressService.UpdateUserProgressAsync(userId, updateDto.ExerciseScore, updateDto.TimeSpent);
            return Ok(progress);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // ===== FRONTEND COMPATIBILITY ENDPOINTS =====
    // These endpoints match exactly what frontend hooks expect

    /// <summary>
    /// Get user stats compatible with useUserStats hook
    /// Frontend expects: { completedExercises, totalExercises, averageScore, ... }
    /// </summary>
    [HttpGet("stats/{userId}")]
    public async Task<ActionResult> GetUserStats(int userId)
    {
        try
        {
            var progress = await _progressService.GetUserProgressAsync(userId);
            if (progress == null)
                return NotFound(new { message = $"User stats for ID {userId} not found" });

            // Format response to match frontend useUserStats hook expectations
            var response = new
            {
                completedExercises = progress.CompletedExercises,
                totalExercises = progress.TotalExercisesAvailable,
                averageScore = (int)Math.Round((decimal)progress.TotalScore),
                totalStudyTime = progress.TotalStudyTime,
                currentStreak = progress.StudyStreak,
                level = progress.Level
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user stats for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get current user progress compatible with useCurrentUserProgress hook
    /// Returns the full user progress object matching frontend expectations
    /// </summary>
    [HttpGet("current-user")]
    public async Task<ActionResult> GetCurrentUserProgress()
    {
        try
        {
            // For now, return user ID 1 (main user)
            // In real app, get from JWT/auth context
            var userId = 1;
            var progress = await _progressService.GetUserProgressAsync(userId);
            
            if (progress == null)
                return NotFound(new { message = "Current user progress not found" });

            // Format response to match frontend UserProgress interface
            var response = new
            {
                id = progress.UserId.ToString(),
                username = progress.Username,
                email = progress.Email,
                totalScore = progress.TotalScore,
                listening = progress.Listening,
                speaking = progress.Speaking,
                reading = progress.Reading,
                writing = progress.Writing,
                exams = progress.Exams,
                completedLessons = progress.CompletedLessons,
                studyStreak = progress.StudyStreak,
                totalStudyTime = progress.TotalStudyTime,
                achievements = progress.Achievements,
                level = progress.Level,
                lastActive = progress.LastActive,
                createdAt = progress.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                updatedAt = progress.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user progress");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get admin-formatted user data compatible with useCurrentUserProgress hook
    /// Returns AdminUser format with totalXp, level conversion, etc.
    /// </summary>
    [HttpGet("admin-format/{userId}")]
    public async Task<ActionResult<AdminUserDto>> GetAdminUser(int userId)
    {
        try
        {
            var progress = await _progressService.GetUserProgressAsync(userId);
            if (progress == null)
                return NotFound(new { message = $"Admin user data for ID {userId} not found" });

            // Convert Level string to integer (extract number from "Level 4" format)
            var levelNumber = 1;
            if (!string.IsNullOrEmpty(progress.Level) && progress.Level.ToLower().StartsWith("level"))
            {
                var levelStr = progress.Level.Replace("Level", "").Replace("level", "").Trim();
                int.TryParse(levelStr, out levelNumber);
            }

            var adminUser = new AdminUserDto
            {
                Id = progress.UserId.ToString(),
                Username = progress.Username,
                Email = progress.Email,
                TotalXp = (int)progress.TotalScore, // Map TotalScore to TotalXp
                Level = levelNumber.ToString(),
                AverageScore = progress.Reading, // Use Reading score for Reading-focused app
                ExercisesCompleted = progress.Exams,
                StreakDays = progress.StudyStreak,
                Achievements = progress.Achievements?.ToList() ?? new List<string>(),
                LastActive = progress.LastActive,
                CreatedAt = progress.CreatedAt,
                UpdatedAt = progress.UpdatedAt
            };

            return Ok(adminUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin user data for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get recent activities compatible with useRecentActivities hook
    /// Returns activity history for Progress page display
    /// </summary>
    [HttpGet("recent-activities/{userId}")]
    public async Task<ActionResult<IEnumerable<RecentActivityDto>>> GetRecentActivities(int userId, [FromQuery] int limit = 20)
    {
        try
        {
            var activities = await _progressService.GetUserActivitiesAsync(userId, limit);
            
            // Convert to frontend-compatible format
            var recentActivities = activities.Select(activity => new RecentActivityDto
            {
                Id = activity.Id,
                Type = activity.Type,
                Topic = activity.Topic ?? "Reading Exercise",
                Timestamp = activity.Date,
                Score = activity.Score,
                Duration = activity.Duration,
                AssignmentType = activity.AssignmentType
            });

            return Ok(recentActivities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent activities for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }


}