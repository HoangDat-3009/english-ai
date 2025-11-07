using EngAce.Api.DTO.Core;
using EngAce.Api.Services.Interfaces;
using Entities.Data;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly ApplicationDbContext _context;

    public LeaderboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(string? timeFilter = null, string? skill = null)
    {
        var query = _context.Users
            .Where(u => u.IsActive)
            .Join(_context.UserProgresses,
                u => u.Id,
                p => p.UserId,
                (u, p) => new { User = u, Progress = p });

        // Apply time filter
        if (!string.IsNullOrEmpty(timeFilter))
        {
            var filterDate = timeFilter.ToLower() switch
            {
                "today" => DateTime.UtcNow.Date,
                "week" => DateTime.UtcNow.AddDays(-7),
                "month" => DateTime.UtcNow.AddDays(-30),
                _ => DateTime.MinValue
            };

            if (filterDate != DateTime.MinValue)
            {
                query = query.Where(up => up.User.LastActiveAt >= filterDate);
            }
        }

        var userProgressData = await query.ToListAsync();

        // Sort by skill or total score
        var leaderboard = userProgressData.Select(up => new LeaderboardEntryDto
        {
            UserId = up.User.Id.ToString(),
            Username = up.User.Username,
            FullName = up.User.FullName,
            Level = up.User.Level,
            TotalXP = up.User.TotalXP,
            ListeningScore = up.Progress.ListeningScore,
            SpeakingScore = up.Progress.SpeakingScore,
            ReadingScore = up.Progress.ReadingScore,
            WritingScore = up.Progress.WritingScore,
            TotalScore = up.Progress.TotalScore,
            StudyStreak = up.User.StudyStreak,
            CompletedExercises = up.Progress.CompletedExercises,
            AverageAccuracy = up.Progress.AverageAccuracy,
            LastActive = up.User.LastActiveAt
        });

        // Sort by specific skill or total score
        leaderboard = skill?.ToLower() switch
        {
            "listening" => leaderboard.OrderByDescending(l => l.ListeningScore),
            "speaking" => leaderboard.OrderByDescending(l => l.SpeakingScore),
            "reading" => leaderboard.OrderByDescending(l => l.ReadingScore),
            "writing" => leaderboard.OrderByDescending(l => l.WritingScore),
            _ => leaderboard.OrderByDescending(l => l.TotalXP)
        };

        // Add ranks
        var rankedLeaderboard = leaderboard
            .Select((entry, index) => 
            {
                entry.Rank = index + 1;
                return entry;
            })
            .ToList();

        return rankedLeaderboard;
    }

    public async Task<UserRankDto?> GetUserRankAsync(int userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null) return null;

        var totalUsers = await _context.Users.CountAsync(u => u.IsActive);
        
        var usersWithHigherXP = await _context.Users
            .CountAsync(u => u.IsActive && u.TotalXP > user.TotalXP);

        var currentRank = usersWithHigherXP + 1;
        var percentile = totalUsers > 0 ? 
            Math.Round((double)(totalUsers - currentRank + 1) / totalUsers * 100, 1) : 0;

        return new UserRankDto
        {
            UserId = userId.ToString(),
            Username = user.Username,
            CurrentRank = currentRank,
            TotalUsers = totalUsers,
            TotalXP = user.TotalXP,
            Percentile = percentile
        };
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetTopUsersAsync(int count = 10)
    {
        var topUsers = await _context.Users
            .Where(u => u.IsActive)
            .Join(_context.UserProgresses,
                u => u.Id,
                p => p.UserId,
                (u, p) => new { User = u, Progress = p })
            .OrderByDescending(up => up.User.TotalXP)
            .Take(count)
            .ToListAsync();

        return topUsers.Select((up, index) => new LeaderboardEntryDto
        {
            Rank = index + 1,
            UserId = up.User.Id.ToString(),
            Username = up.User.Username,
            FullName = up.User.FullName,
            Level = up.User.Level,
            TotalXP = up.User.TotalXP,
            ListeningScore = up.Progress.ListeningScore,
            SpeakingScore = up.Progress.SpeakingScore,
            ReadingScore = up.Progress.ReadingScore,
            WritingScore = up.Progress.WritingScore,
            TotalScore = up.Progress.TotalScore,
            StudyStreak = up.User.StudyStreak,
            CompletedExercises = up.Progress.CompletedExercises,
            AverageAccuracy = up.Progress.AverageAccuracy,
            LastActive = up.User.LastActiveAt
        });
    }

    public async Task<LeaderboardStatsDto> GetLeaderboardStatsAsync()
    {
        var totalUsers = await _context.Users.CountAsync(u => u.IsActive);
        
        var today = DateTime.UtcNow.Date;
        var activeUsersToday = await _context.Users
            .CountAsync(u => u.IsActive && u.LastActiveAt >= today);

        var averageScore = await _context.UserProgresses
            .AverageAsync(p => (double?)p.TotalScore) ?? 0;

        var totalExercisesCompleted = await _context.ReadingExerciseResults
            .CountAsync(r => r.IsCompleted);

        return new LeaderboardStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsersToday = activeUsersToday,
            AverageScore = (double)averageScore,
            TotalExercisesCompleted = totalExercisesCompleted
        };
    }
}