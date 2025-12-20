using EngAce.Api.DTO.Core;
using EngAce.Api.Helpers;
using EngAce.Api.Services.Exercise;
using Entities.Data;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Services.Exercise;

public class LeaderboardService : ILeaderboardService
{
    private readonly ApplicationDbContext _context;

    public LeaderboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(string? timeFilter = null, string? skill = null)
    {
        // Load users and completions separately (same approach as ProgressService)
        var users = await _context.Users
            .Where(u => u.Status == "active" || u.Status == null) // Include users without status or active
            .ToListAsync();

        // Filter completions the same way as ProgressService - only completed exercises
        var allCompletions = await _context.Completions
            .Where(c => c.CompletedAt.HasValue)
            .Include(c => c.Exercise)
            .ToListAsync();

        // Apply time filter to users
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
                users = users.Where(u => u.LastActiveAt >= filterDate).ToList();
            }
        }

        // Group completions by userId (client-side join, same as ProgressService approach)
        var userProgressData = users.Select(u => new
        {
            User = u,
            Completions = allCompletions.Where(c => c.UserId == u.Id).ToList()
        }).ToList();

        // Sort by skill or total score
        var leaderboard = userProgressData.Select(uc =>
        {
            var completionList = uc.Completions?.ToList() ?? new List<Completion>();
            var toeicParts = ToeicPartHelper.BuildPartScores(completionList);
            var listeningScore = ToeicPartHelper.SumListening(toeicParts);
            var readingScore = ToeicPartHelper.SumReading(toeicParts);

            return new LeaderboardEntryDto
            {
                UserId = uc.User.Id,
                Username = uc.User.Username,
                FullName = uc.User.FullName,
                Level = UserProfileHelper.GetProfileTier(uc.User.TotalXp),
                TotalXP = uc.User.TotalXp,
                ListeningScore = (int)Math.Round(listeningScore),
                SpeakingScore = 0,
                ReadingScore = (int)Math.Round(readingScore),
                WritingScore = 0,
                TotalScore = completionList.Any() ? (int)completionList.Sum(c => c.Score) : 0,
                StudyStreak = UserProfileHelper.CalculateStudyStreak(completionList),
                CompletedExercises = completionList.Count,
                AverageScore = completionList.Any() ? (double)completionList.Average(c => c.Score ?? 0) : 0,
                AverageAccuracy = completionList.Any() ? (decimal)completionList.Average(c => c.Score ?? 0) : 0,
                LastActive = uc.User.LastActiveAt ?? DateTime.UtcNow,
                LastActivity = uc.User.LastActiveAt ?? DateTime.UtcNow,
                ToeicParts = toeicParts
            };
        });

        var normalizedFilter = skill?.ToLowerInvariant();
        if (ToeicPartHelper.IsPartFilter(normalizedFilter))
        {
            leaderboard = leaderboard.OrderByDescending(entry =>
                ToeicPartHelper.GetPartScore(entry.ToeicParts, normalizedFilter));
        }
        else
        {
            leaderboard = normalizedFilter switch
            {
                "listening" => leaderboard.OrderByDescending(entry => ToeicPartHelper.SumListening(entry.ToeicParts)),
                "reading" => leaderboard.OrderByDescending(entry => ToeicPartHelper.SumReading(entry.ToeicParts)),
                _ => leaderboard.OrderByDescending(entry => entry.TotalXP)
            };
        }

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
            .FirstOrDefaultAsync(u => u.Id == userId && u.Status == "active");

        if (user == null) return null;

        var totalUsers = await _context.Users.CountAsync(u => u.Status == "active");
        var userCompletions = await _context.Completions
            .Where(c => c.UserId == userId && c.CompletedAt.HasValue)
            .Include(c => c.Exercise)
            .ToListAsync();
        var toeicParts = ToeicPartHelper.BuildPartScores(userCompletions);
        
        var usersWithHigherXP = await _context.Users
            .CountAsync(u => u.Status == "active" && u.TotalXp > user.TotalXp);

        var currentRank = usersWithHigherXP + 1;
        var percentile = totalUsers > 0 ? 
            Math.Round((double)(totalUsers - currentRank + 1) / totalUsers * 100, 1) : 0;

        return new UserRankDto
        {
            UserId = user.Id,
            Username = user.Username,
            CurrentRank = currentRank,
            TotalUsers = totalUsers,
            TotalXP = user.TotalXp,
            Percentile = percentile,
            ToeicParts = toeicParts
        };
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetTopUsersAsync(int count = 10)
    {
        // Load users and completions separately (same approach as ProgressService)
        var users = await _context.Users
            .Where(u => u.Status == "active" || u.Status == null) // Include users without status or active
            .OrderByDescending(u => u.TotalXp)
            .Take(count)
            .ToListAsync();

        // Filter completions the same way as ProgressService - only completed exercises
        var allCompletions = await _context.Completions
            .Where(c => c.CompletedAt.HasValue)
            .Include(c => c.Exercise)
            .ToListAsync();

        // Group completions by userId (client-side join, same as ProgressService approach)
        var topUsers = users.Select(u => new
        {
            User = u,
            Completions = allCompletions.Where(c => c.UserId == u.Id).ToList()
        }).ToList();

        return topUsers.Select((uc, index) =>
        {
            var completionList = uc.Completions ?? new List<Completion>();
            var toeicParts = ToeicPartHelper.BuildPartScores(completionList);

            return new LeaderboardEntryDto
            {
                Rank = index + 1,
                UserId = uc.User.Id,
                Username = uc.User.Username,
                FullName = uc.User.FullName,
                Level = UserProfileHelper.GetProfileTier(uc.User.TotalXp),
                TotalXP = uc.User.TotalXp,
                ListeningScore = (int)Math.Round(ToeicPartHelper.SumListening(toeicParts)),
                SpeakingScore = 0,
                ReadingScore = (int)Math.Round(ToeicPartHelper.SumReading(toeicParts)),
                WritingScore = 0,
                TotalScore = completionList.Any() ? (int)completionList.Sum(c => c.Score) : 0,
                StudyStreak = UserProfileHelper.CalculateStudyStreak(completionList),
                CompletedExercises = completionList.Count,
                AverageAccuracy = completionList.Any() ? (decimal)completionList.Average(c => c.Score) : 0,
                LastActive = uc.User.LastActiveAt ?? DateTime.UtcNow,
                ToeicParts = toeicParts
            };
        });
    }

    public async Task<LeaderboardStatsDto> GetLeaderboardStatsAsync()
    {
        var totalUsers = await _context.Users.CountAsync(u => u.Status == "active");
        
        var today = DateTime.UtcNow.Date;
        var activeUsersToday = await _context.Users
            .CountAsync(u => u.Status == "active" && u.LastActiveAt >= today);

        var averageScore = await _context.Completions
            .AverageAsync(c => (double?)c.Score) ?? 0;

        var totalExercisesCompleted = await _context.Completions
            .CountAsync(c => c.CompletedAt.HasValue);

        return new LeaderboardStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsersToday = activeUsersToday,
            AverageScore = (double)averageScore,
            TotalExercisesCompleted = totalExercisesCompleted
        };
    }
}