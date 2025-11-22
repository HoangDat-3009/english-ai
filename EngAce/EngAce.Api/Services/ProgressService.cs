using EngAce.Api.DTO.Core;
using EngAce.Api.Helpers;
using EngAce.Api.Services.Interfaces;
using Entities.Data;
using Microsoft.EntityFrameworkCore;

namespace EngAce.Api.Services;

public class ProgressService : IProgressService
{
    private readonly ApplicationDbContext _context;

    public ProgressService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserProgressDto?> GetUserProgressAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        // Get user achievements (if Achievement table exists)
        var achievements = new List<string>(); // Simplified for now
        // var achievements = await _context.Achievements
        //     .Where(a => a.UserId == userId)
        //     .Select(a => a.Title)
        //     .ToListAsync();

        var userCompletions = await _context.Completions
            .Where(c => c.UserId == userId && c.CompletedAt.HasValue)
            .Include(c => c.Exercise)
            .ToListAsync();

        var completedExercises = userCompletions.Count;
        var uniqueExercises = userCompletions
            .Select(c => c.ExerciseId)
            .Distinct()
            .Count();
        var averageScore = userCompletions.Any()
            ? (double)userCompletions.Average(c => c.Score)
            : 0;

        var toeicParts = ToeicPartHelper.BuildPartScores(userCompletions);
        var listeningScore = ToeicPartHelper.SumListening(toeicParts);
        var readingScore = ToeicPartHelper.SumReading(toeicParts);
        var profileTier = UserProfileHelper.GetProfileTier(user.TotalXp);
        var studyStreak = UserProfileHelper.CalculateStudyStreak(userCompletions);

        return new UserProgressDto
        {
            Id = user.Id.ToString(),
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Level = profileTier,
            CompletedExercises = completedExercises,
            TotalExercisesAvailable = await _context.Exercises.CountAsync(e => e.IsActive),
            AverageAccuracy = averageScore,
            ListeningAccuracy = listeningScore,
            ReadingAccuracy = readingScore,
            WeeklyGoal = 5, // Default value
            MonthlyGoal = 20, // Default value
            StudyStreak = studyStreak,
            TotalStudyTime = TimeSpan.FromMinutes(user.TotalStudyTime),
            TotalXP = user.TotalXp,
            CurrentStreak = studyStreak,
            Listening = (int)Math.Round(listeningScore),
            Speaking = 0,
            Reading = (int)Math.Round(readingScore),
            Writing = 0,
            Exams = uniqueExercises,
            TotalScore = (int)Math.Round(averageScore),
            LastActivity = user.LastActiveAt ?? DateTime.UtcNow,
            LastActive = user.LastActiveAt ?? DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow,
            CreatedAt = user.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            ToeicParts = toeicParts
        };
    }

    public async Task<WeeklyProgressDto> GetWeeklyProgressAsync(int userId)
    {
        // Get this week's completions
        var startOfWeek = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);

        var thisWeekCompletions = await _context.Completions
            .Where(c => c.UserId == userId && 
                       c.CompletedAt.HasValue &&
                       c.CompletedAt.Value >= startOfWeek && 
                       c.CompletedAt.Value < endOfWeek)
            .ToListAsync();

        var completedThisWeek = thisWeekCompletions.Count;
        var weeklyGoal = 5; // Default weekly goal
        var progressPercentage = weeklyGoal > 0 ? 
            Math.Min(100, (decimal)completedThisWeek / weeklyGoal * 100) : 0;

        // Generate daily progress for the week
        var dailyProgress = new List<DailyProgressDto>();
        for (int i = 0; i < 7; i++)
        {
            var day = startOfWeek.AddDays(i);
            var dayCompletions = thisWeekCompletions
                .Where(c => c.CompletedAt.HasValue && c.CompletedAt.Value.Date == day.Date)
                .ToList();
            
            var estimatedTimeMinutes = dayCompletions.Sum(c => c.TimeSpent?.TotalMinutes ?? 30);
            
            dailyProgress.Add(new DailyProgressDto
            {
                // Frontend compatible format
                Day = $"T{i + 1}", // Frontend expects "T1", "T2", etc.
                Exercises = dayCompletions.Count,
                Time = TimeSpan.FromMinutes(estimatedTimeMinutes),
                
                // Keep original properties for backward compatibility
                Date = day,
                ExercisesCompleted = dayCompletions.Count,
                TimeSpentMinutes = (int)estimatedTimeMinutes,
                XPEarned = (int)dayCompletions.Sum(c => c.Score)
            });
        }

        return new WeeklyProgressDto
        {
            WeeklyGoal = weeklyGoal,
            CompletedThisWeek = completedThisWeek,
            ProgressPercentage = (double)progressPercentage,
            DailyProgress = dailyProgress
        };
    }

    public async Task<IEnumerable<ActivityDto>> GetUserActivitiesAsync(int userId, int limit = 10)
    {
        var completions = await _context.Completions
            .Include(c => c.Exercise)
            .Where(c => c.UserId == userId && c.CompletedAt.HasValue)
            .OrderByDescending(c => c.CompletedAt)
            .Take(limit)
            .ToListAsync();

        var activities = new List<ActivityDto>();

        // Add completions as activities - format for frontend compatibility
        activities.AddRange(completions.Select(c => new ActivityDto
        {
            Id = c.CompletionId.ToString(),
            Type = "Reading Exercise", // Frontend expects 'Type'
            Topic = c.Exercise?.Title ?? "Reading Exercise", // Frontend expects 'Topic'
            Timestamp = c.CompletedAt ?? c.StartedAt ?? DateTime.UtcNow, // Frontend expects DateTime
            Score = c.Score.HasValue ? (int?)Math.Round(c.Score.Value) : null, // Convert decimal? to int?
            Duration = c.TimeSpent ?? TimeSpan.FromMinutes(30), // Frontend expects 'Duration'
            AssignmentType = c.Exercise?.Type ?? "Part 7", // Frontend expects this field
            TimeSpentMinutes = (int)(c.TimeSpent?.TotalMinutes ?? 30),
            XPEarned = c.Score.HasValue ? (int)Math.Round(c.Score.Value) : 0,
            Status = "Completed"
        }));

        return activities.OrderByDescending(a => a.Timestamp).Take(limit);
    }

    public async Task<UserProgressDto> UpdateUserProgressAsync(int userId, int exerciseScore, int timeSpent)
    {
        // Since UserProgresses table doesn't exist in new schema, create a simple completion record
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            throw new ArgumentException($"User not found for userId: {userId}");

        // Update user stats directly
        user.TotalXp += exerciseScore;

        await _context.SaveChangesAsync();

        // Return simulated progress data based on user's completions
        var completions = await _context.Completions
            .Where(c => c.UserId == userId)
            .Include(c => c.Exercise)
            .ToListAsync();
        var studyStreak = UserProfileHelper.CalculateStudyStreak(completions);

        return new UserProgressDto
        {
            UserId = userId,
            CompletedExercises = completions.Count,
            TotalScore = (int)completions.Sum(c => c.Score),
            Reading = completions.Count > 0 ? (int)completions.Max(c => c.Score) : 0,
            TotalStudyTime = TimeSpan.FromMinutes(user.TotalStudyTime),
            StudyStreak = studyStreak,
            LastUpdated = DateTime.UtcNow,
            CurrentStreak = studyStreak
        };
    }
}