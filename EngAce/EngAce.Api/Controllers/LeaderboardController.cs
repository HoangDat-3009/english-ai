using EngAce.Api.DTO.Core;
using EngAce.Api.DTO.Shared;
using EngAce.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EngAce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;
    private readonly ILogger<LeaderboardController> _logger;

    public LeaderboardController(ILeaderboardService leaderboardService, ILogger<LeaderboardController> logger)
    {
        _leaderboardService = leaderboardService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetLeaderboard(
        [FromQuery] string? timeFilter = null, 
        [FromQuery] string? skill = null)
    {
        try
        {
            var leaderboard = await _leaderboardService.GetLeaderboardAsync(timeFilter, skill);
            return Ok(leaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard with filter: {TimeFilter}, skill: {Skill}", timeFilter, skill);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("user/{userId}/rank")]
    public async Task<ActionResult<UserRankDto>> GetUserRank(int userId)
    {
        try
        {
            var userRank = await _leaderboardService.GetUserRankAsync(userId);
            if (userRank == null)
                return NotFound(new { message = $"User rank for ID {userId} not found" });

            return Ok(userRank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user rank for user {UserId}", userId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("top")]
    public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetTopUsers([FromQuery] int count = 10)
    {
        try
        {
            var topUsers = await _leaderboardService.GetTopUsersAsync(count);
            return Ok(topUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top {Count} users", count);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult<LeaderboardStatsDto>> GetLeaderboardStats()
    {
        try
        {
            var stats = await _leaderboardService.GetLeaderboardStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard stats");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get leaderboard in admin format compatible with useAdminLeaderboard hook
    /// Returns LeaderboardUser[] format with proper field mapping for frontend
    /// </summary>
    [HttpGet("admin-format")]
    public async Task<ActionResult<List<LeaderboardUserDto>>> GetAdminLeaderboard(
        [FromQuery] string timeFilter = "all")
    {
        try
        {
            // Get base leaderboard data
            var leaderboard = await _leaderboardService.GetLeaderboardAsync(timeFilter, null);
            
            // Convert to admin format for frontend compatibility
            var adminLeaderboard = leaderboard.Select((entry, index) => new LeaderboardUserDto
            {
                Rank = index + 1, // 1-based ranking
                UserId = entry.UserId,
                Username = entry.Username,
                FullName = entry.FullName,
                TotalXP = entry.TotalXP,
                Level = entry.Level,
                AverageScore = entry.AverageScore,
                ExercisesCompleted = entry.ExercisesCompleted,
                StudyStreak = entry.StudyStreak,
                Badge = entry.Badge
            }).ToList();

            return Ok(adminLeaderboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin leaderboard with filter: {TimeFilter}", timeFilter);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get filtered leaderboard with time-based filtering
    /// Supports today, week, month, all filters for frontend compatibility
    /// </summary>
    [HttpGet("filtered")]
    public async Task<ActionResult<List<LeaderboardUserDto>>> GetFilteredLeaderboard(
        [FromQuery] string timeFilter = "all",
        [FromQuery] string skill = "total")
    {
        try
        {
            var leaderboard = await _leaderboardService.GetLeaderboardAsync(timeFilter, skill);
            
            // Apply time-based filtering logic
            var filteredLeaderboard = ApplyTimeFilter(leaderboard, timeFilter);
            
            // Convert to admin format
            var result = filteredLeaderboard.Select((entry, index) => new LeaderboardUserDto
            {
                Rank = index + 1,
                Username = entry.Username,
                TotalScore = (int)entry.TotalXP,
                Listening = (int)(entry.TotalXP * 0.5m),
                Speaking = (int)(entry.TotalXP * 0.25m), 
                Reading = (int)entry.AverageScore,
                Writing = (int)(entry.TotalXP * 0.05m),
                Exams = entry.ExercisesCompleted,
                LastUpdate = entry.LastActivity
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting filtered leaderboard");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Apply time filtering to leaderboard entries
    /// </summary>
    private IEnumerable<LeaderboardEntryDto> ApplyTimeFilter(IEnumerable<LeaderboardEntryDto> leaderboard, string timeFilter)
    {
        var now = DateTime.UtcNow;
        
        return timeFilter.ToLower() switch
        {
            "today" => leaderboard.Where(entry => entry.LastActivity.Date == now.Date),
            "week" => leaderboard.Where(entry => entry.LastActivity >= now.AddDays(-7)),
            "month" => leaderboard.Where(entry => entry.LastActivity >= now.AddDays(-30)),
            _ => leaderboard // "all" or default
        };
    }
}