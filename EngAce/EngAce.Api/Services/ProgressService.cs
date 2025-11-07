using EngAce.Api.DTO.Core;
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

        var progress = await _context.UserProgresses.FindAsync(userId);
        if (progress == null) return null;

        // Get user achievements
        var achievements = await _context.Achievements
            .Where(a => a.UserId == userId)
            .Select(a => a.Title) // Use 'Title' instead of 'Name'
            .ToListAsync();

        // Calculate completed lessons from exercise results
        var completedLessons = await _context.ReadingExerciseResults
            .Where(r => r.UserId == userId && r.IsCompleted)
            .CountAsync();

        // Calculate exams count (unique exercises completed)
        var exams = await _context.ReadingExerciseResults
            .Where(r => r.UserId == userId && r.IsCompleted)
            .Select(r => r.ReadingExerciseId) // Use 'ReadingExerciseId' instead of 'ExerciseId'
            .Distinct()
            .CountAsync();

        return new UserProgressDto
        {
            UserId = userId.ToString(),
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Level = user.Level,
            
            // TOEIC Scores (frontend expects these exact property names)
            TotalScore = progress.TotalScore,
            Listening = progress.ListeningScore,
            Speaking = progress.SpeakingScore,
            Reading = progress.ReadingScore,
            Writing = progress.WritingScore,
            
            // Progress stats
            Exams = exams,
            CompletedLessons = completedLessons,
            CompletedExercises = progress.CompletedExercises,
            TotalExercisesAvailable = progress.TotalExercisesAvailable,
            AverageAccuracy = (double)progress.AverageAccuracy,
            ListeningAccuracy = (double)progress.ListeningAccuracy,
            ReadingAccuracy = (double)progress.ReadingAccuracy,
            CurrentStreak = progress.CurrentStreak,
            WeeklyGoal = progress.WeeklyGoal,
            MonthlyGoal = progress.MonthlyGoal,
            
            // User info
            StudyStreak = user.StudyStreak,
            TotalStudyTime = TimeSpan.FromMinutes(user.TotalStudyTime),
            TotalXP = user.TotalXP,
            Achievements = achievements,
            LastActive = DateTime.Parse(user.LastActiveAt.ToString()),
            
            // Timestamps
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.LastActiveAt,
            LastUpdated = progress.LastUpdated
        };
    }

    public async Task<WeeklyProgressDto> GetWeeklyProgressAsync(int userId)
    {
        var progress = await _context.UserProgresses.FindAsync(userId);
        if (progress == null)
        {
            return new WeeklyProgressDto { WeeklyGoal = 5, CompletedThisWeek = 0, ProgressPercentage = 0 };
        }

        // Get this week's sessions
        var startOfWeek = DateTime.UtcNow.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);

        var thisWeekSessions = await _context.StudySessions
            .Where(s => s.UserId == userId && 
                       s.StartTime >= startOfWeek && 
                       s.StartTime < endOfWeek &&
                       s.IsCompleted)
            .ToListAsync();

        var completedThisWeek = thisWeekSessions.Count;
        var progressPercentage = progress.WeeklyGoal > 0 ? 
            Math.Min(100, (decimal)completedThisWeek / progress.WeeklyGoal * 100) : 0;

        // Generate daily progress for the week
        var dailyProgress = new List<DailyProgressDto>();
        for (int i = 0; i < 7; i++)
        {
            var day = startOfWeek.AddDays(i);
            var daySessions = thisWeekSessions.Where(s => s.StartTime.Date == day.Date).ToList();
            
            dailyProgress.Add(new DailyProgressDto
            {
                // Frontend compatible format
                Day = $"T{i + 1}", // Frontend expects "T1", "T2", etc.
                Exercises = daySessions.Sum(s => s.ExercisesCompleted),
                Time = TimeSpan.FromMinutes(daySessions.Sum(s => s.DurationMinutes)),
                
                // Keep original properties for backward compatibility
                Date = day,
                ExercisesCompleted = daySessions.Sum(s => s.ExercisesCompleted),
                TimeSpentMinutes = daySessions.Sum(s => s.DurationMinutes),
                XPEarned = daySessions.Sum(s => s.XPEarned)
            });
        }

        return new WeeklyProgressDto
        {
            WeeklyGoal = progress.WeeklyGoal,
            CompletedThisWeek = completedThisWeek,
            ProgressPercentage = (double)progressPercentage,
            DailyProgress = dailyProgress
        };
    }

    public async Task<IEnumerable<ActivityDto>> GetUserActivitiesAsync(int userId, int limit = 10)
    {
        var sessions = await _context.StudySessions
            .Where(s => s.UserId == userId && s.IsCompleted)
            .OrderByDescending(s => s.EndTime)
            .Take(limit)
            .ToListAsync();

        var results = await _context.ReadingExerciseResults
            .Include(r => r.ReadingExercise)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CompletedAt)
            .Take(limit)
            .ToListAsync();

        var activities = new List<ActivityDto>();

        // Add study sessions - format for frontend compatibility
        activities.AddRange(sessions.Select(s => new ActivityDto
        {
            Id = s.Id.ToString(),
            Type = s.ActivityType, // Frontend expects 'Type'
            Topic = s.ActivityName ?? s.ActivityType, // Frontend expects 'Topic'
            Timestamp = s.EndTime ?? s.StartTime, // Frontend expects DateTime
            Score = 85, // Default score for sessions (you can calculate based on performance)
            Duration = TimeSpan.FromMinutes(s.DurationMinutes), // Frontend expects 'Duration'
            AssignmentType = s.ActivityType, // Frontend expects this field
            TimeSpentMinutes = s.DurationMinutes,
            XPEarned = s.XPEarned,
            Status = "Completed"
        }));

        // Add reading exercise results - format for frontend compatibility
        activities.AddRange(results.Select(r => new ActivityDto
        {
            Id = r.Id.ToString(),
            Type = "Reading Exercise", // Frontend expects 'Type'
            Topic = r.ReadingExercise.Name, // Frontend expects 'Topic'
            Timestamp = r.CompletedAt, // Frontend expects DateTime
            Score = r.Score, // Frontend expects non-nullable int
            Duration = r.TimeSpent, // Frontend expects 'Duration'
            AssignmentType = "Reading", // Frontend expects this field
            TimeSpentMinutes = (int)r.TimeSpent.TotalMinutes,
            XPEarned = r.Score / 10, // Simple XP calculation
            Status = r.IsCompleted ? "Completed" : "In Progress"
        }));

        // Sort by date (convert back from string for sorting, then return as is)
        return activities.OrderByDescending(a => a.Date).Take(limit);
    }

    public async Task<UserProgressDto> UpdateUserProgressAsync(int userId, int exerciseScore, int timeSpent)
    {
        var user = await _context.Users.FindAsync(userId);
        var progress = await _context.UserProgresses.FindAsync(userId);

        if (user == null || progress == null)
            throw new ArgumentException($"User or progress not found for userId: {userId}");

        // Update progress
        progress.CompletedExercises++;
        progress.TotalScore += exerciseScore;
        progress.ReadingScore = Math.Max(progress.ReadingScore, exerciseScore);
        progress.LastUpdated = DateTime.UtcNow;

        // Update user stats
        user.TotalStudyTime += timeSpent;
        user.TotalXP += exerciseScore;
        user.LastActiveAt = DateTime.UtcNow;

        // Update streak (simplified logic)
        var lastSession = await _context.StudySessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.EndTime)
            .FirstOrDefaultAsync();

        if (lastSession == null || 
            (DateTime.UtcNow - (lastSession.EndTime ?? lastSession.StartTime)).TotalDays <= 1)
        {
            user.StudyStreak++;
            progress.CurrentStreak = user.StudyStreak;
        }

        await _context.SaveChangesAsync();

        return (await GetUserProgressAsync(userId))!;
    }
}