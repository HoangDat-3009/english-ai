using EngAce.Api.DTO.Core;
using EngAce.Api.DTO.Shared;
using EngAce.Api.Helpers;
using EngAce.Api.Services.Exercise;
using Entities.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LeaderboardController> _logger;

    public LeaderboardController(
        ILeaderboardService leaderboardService, 
        ApplicationDbContext context,
        ILogger<LeaderboardController> logger)
    {
        _leaderboardService = leaderboardService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetLeaderboard(
        [FromQuery] string? timeFilter = null, 
        [FromQuery] string? skill = null)
    {
        try
        {
            var leaderboard = await _leaderboardService.GetLeaderboardAsync(timeFilter, skill);
            var leaderboardList = leaderboard.ToList();
            
            // Format response to match frontend LeaderboardResponse interface
            var response = new
            {
                users = leaderboardList.Select(entry =>
                {
                    var parts = entry.ToeicParts.Select(part => new
                    {
                        key = part.Key,
                        part = part.Part,
                        label = part.Label,
                        title = part.Title,
                        skill = part.Skill,
                        description = part.Description,
                        questionTypes = part.QuestionTypes,
                        score = part.Score,
                        attempts = part.Attempts
                    }).ToList();

                    return new
                    {
                        rank = entry.Rank,
                        username = entry.Username,
                        totalScore = entry.TotalScore,
                        listening = entry.ListeningScore,
                        speaking = entry.SpeakingScore,
                        reading = entry.ReadingScore,
                        writing = entry.WritingScore,
                        exams = entry.CompletedExercises,
                        parts,
                        lastUpdate = entry.LastActive.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                    };
                }).ToList(),
                totalCount = leaderboardList.Count,
                timeFilter = timeFilter ?? "all",
                category = skill ?? "total",
                lastUpdated = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard with filter: {TimeFilter}, skill: {Skill}", timeFilter, skill);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("user/{userId}/rank")]
    public async Task<ActionResult<object>> GetUserRank(int userId)
    {
        try
        {
            var userRank = await _leaderboardService.GetUserRankAsync(userId);
            if (userRank == null)
                return NotFound(new { message = $"User rank for ID {userId} not found" });

            // Get user details and scores
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = $"User with ID {userId} not found" });

            // Get user completions for skill scores
            var completions = await _context.Completions
                .Where(c => c.UserId == userId && c.CompletedAt.HasValue)
                .Include(c => c.Exercise)
                .ToListAsync();

            var toeicParts = ToeicPartHelper.BuildPartScores(completions);
            var listeningScore = (int)Math.Round(ToeicPartHelper.SumListening(toeicParts));
            var readingScore = (int)Math.Round(ToeicPartHelper.SumReading(toeicParts));

            // Format response to match frontend UserRank interface
            var response = new
            {
                userId = userId.ToString(),
                username = user.Username,
                totalScore = user.TotalXp, // Use TotalXP as totalScore
                listening = listeningScore,
                speaking = 0,
                reading = readingScore,
                writing = 0,
                rank = userRank.CurrentRank,
                percentile = userRank.Percentile,
                parts = toeicParts.Select(part => new
                {
                    key = part.Key,
                    part = part.Part,
                    label = part.Label,
                    title = part.Title,
                    skill = part.Skill,
                    description = part.Description,
                    questionTypes = part.QuestionTypes,
                    score = part.Score,
                    attempts = part.Attempts
                })
            };

            return Ok(response);
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
                Exams = entry.CompletedExercises,
                StudyStreak = entry.StudyStreak,
                Badge = entry.Badge,
                ToeicParts = entry.ToeicParts
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
            var result = filteredLeaderboard.Select((entry, index) =>
            {
                var listening = (int)Math.Round(ToeicPartHelper.SumListening(entry.ToeicParts));
                var reading = (int)Math.Round(ToeicPartHelper.SumReading(entry.ToeicParts));

                return new LeaderboardUserDto
                {
                    Rank = index + 1,
                    Username = entry.Username,
                    TotalScore = (int)entry.TotalXP,
                    Listening = listening,
                    Speaking = 0,
                    Reading = reading,
                    Writing = 0,
                    Exams = entry.CompletedExercises,
                    LastUpdate = entry.LastActivity,
                    ToeicParts = entry.ToeicParts
                };
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