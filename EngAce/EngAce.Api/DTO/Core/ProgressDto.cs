using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EngAce.Api.DTO.Core;

// ===== STANDARDIZED PROGRESS DTOs =====
// Hợp nhất từ UserProgressDto.cs và AdminDto.cs

/// <summary>
/// DTO cho user progress - hợp nhất từ UserProgressDto.cs và các DTOs khác
/// Frontend compatible with UserStats interface
/// </summary>
public class UserProgressDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int CompletedExercises { get; set; }
    public int TimeSpentToday { get; set; }
    public DateTime LastActivity { get; set; }
    
    // Properties từ original AdminDto
    public int Rank { get; set; }
    public double ProgressPercentage { get; set; }
    
    // User information properties used in controllers/services
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    
    // Skill scores
    public int Listening { get; set; }
    public int Speaking { get; set; }
    public int Reading { get; set; }
    public int Writing { get; set; }
    public int Exams { get; set; }
    
    // Progress tracking
    public int CompletedLessons { get; set; }
    public int TotalExercisesAvailable { get; set; }
    public double AverageAccuracy { get; set; }
    public double ListeningAccuracy { get; set; }
    public double ReadingAccuracy { get; set; }
    
    // Goals and tracking
    public int WeeklyGoal { get; set; }
    public int MonthlyGoal { get; set; }
    public int StudyStreak { get; set; }
    public TimeSpan TotalTimeSpent { get; set; }
    public int TotalXP { get; set; }
    public List<string> Achievements { get; set; } = new();
    public DateTime LastActive { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    
    // Missing properties used in services/controllers
    public int TotalScore { get; set; }
    public TimeSpan TotalStudyTime { get; set; }
    public int CurrentStreak { get; set; }
    
    // Additional consolidated properties - TEMPORARILY DISABLED due to JSON conflicts
    // public List<ActivityDto> Activities { get; set; } = new();
    
    // === FRONTEND COMPATIBILITY PROPERTIES ===
    // Match UserStats interface from statsService.ts
    [JsonPropertyName("totalExercises")]
    public int TotalExercises => TotalExercisesAvailable;
    
    [JsonPropertyName("totalStudyTime")]
    public int TotalStudyTimeMinutes => (int)TotalStudyTime.TotalMinutes;
    
    [JsonPropertyName("averageScore")]
    public double AverageScore => AverageAccuracy;
    
    [JsonPropertyName("experiencePoints")]
    public int ExperiencePoints => TotalXP;
    
    [JsonPropertyName("currentStreak")]
    public int CurrentStreakAlias => StudyStreak;
    
    // ID type compatibility for frontend (string to int conversion)
    [JsonPropertyName("userId")]
    public int UserIdInt => int.TryParse(UserId, out var id) ? id : 0;
}

/// <summary>
/// Request DTO để update progress
/// </summary>
public class UpdateProgressDto
{
    public string UserId { get; set; } = string.Empty;
    public string ExerciseId { get; set; } = string.Empty;
    public int Score { get; set; }
    public int ExerciseScore { get; set; } // Alias for Score
    public int TimeSpent { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    
    // Additional properties used in controllers
    public string ExerciseType { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public int XPEarned { get; set; }
}

/// <summary>
/// DTO cho weekly progress - từ UserProgressDto.cs
/// </summary>
public class WeeklyProgressDto
{
    public int Week { get; set; }
    public int Year { get; set; }
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public int CompletedExercises { get; set; }
    public double AverageScore { get; set; }
    public TimeSpan TotalStudyTime { get; set; }
    public List<string> CompletedLevels { get; set; } = new();
    
    // Additional properties used in frontend
    public string Day { get; set; } = string.Empty; // T2, T3, etc.
    public int Exercises { get; set; }
    public TimeSpan Time { get; set; }
    public DateTime Date { get; set; }
    
    // Frontend compatibility - convert TimeSpan to number (minutes)
    [JsonPropertyName("time")]
    public int TimeMinutes => (int)Time.TotalMinutes;
    
    [JsonPropertyName("totalStudyTime")]
    public int TotalStudyTimeMinutes => (int)TotalStudyTime.TotalMinutes;
    
    // Properties used in services
    public int WeeklyGoal { get; set; }
    public int CompletedThisWeek { get; set; }
    public double ProgressPercentage { get; set; }
    public List<DailyProgressDto> DailyProgress { get; set; } = new();
}

/// <summary>
/// DTO cho daily progress - từ UserProgressDto.cs
/// </summary>
public class DailyProgressDto
{
    public DateTime Date { get; set; }
    public int CompletedExercises { get; set; }
    public int ExercisesCompleted { get; set; } // Alias for CompletedExercises
    public double AverageScore { get; set; }
    public TimeSpan StudyTime { get; set; }
    public int TimeSpentMinutes { get; set; }
    public int XPEarned { get; set; }
    public List<string> CompletedTypes { get; set; } = new();
    public bool MetDailyGoal { get; set; }
    
    // Frontend properties
    public string Day { get; set; } = string.Empty;
    public int Exercises { get; set; }
    public TimeSpan Time { get; set; }
    
    // Frontend compatibility - convert TimeSpan to number (minutes)
    [JsonPropertyName("time")]
    public int TimeMinutes => (int)Time.TotalMinutes;
    
    [JsonPropertyName("studyTime")]
    public int StudyTimeMinutes => (int)StudyTime.TotalMinutes;
}

/// <summary>
/// DTO cho activity - từ UserProgressDto.cs và AdminDto.cs RecentActivityDto
/// Frontend compatible with Activity interface
/// </summary>
public class ActivityDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Alias for ActivityType
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    
    // Keep Date property for backward compatibility
    public DateTime Date => Timestamp;
    
    public string Details { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string? ExerciseType { get; set; }
    
    // Additional properties used in services
    public string Topic { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string AssignmentType { get; set; } = string.Empty;
    public int TimeSpentMinutes { get; set; }
    public int XPEarned { get; set; }
    public string Status { get; set; } = string.Empty;
    
    // === FRONTEND COMPATIBILITY PROPERTIES ===
    // Match Activity interface from frontend
    public int MaxScore { get; set; } = 100; // Add maxScore field required by frontend
    
    // DateString property - no JsonPropertyName to avoid conflicts
    public string DateString => Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); // ISO string format
    
    // DurationString property - no JsonPropertyName to avoid conflicts
    public string DurationString => Duration.ToString(@"hh\:mm\:ss"); // String format for duration
    
    // ExerciseType alias property - no JsonPropertyName to avoid conflicts
    public string ExerciseTypeAlias => AssignmentType;
    
    // ID type compatibility for frontend (string to int conversion) - no JsonPropertyName to avoid conflicts
    public int IdInt => int.TryParse(Id, out var id) ? id : 0;
    
    public int UserIdInt => int.TryParse(UserId, out var userId) ? userId : 0;
}



/// <summary>
/// Request DTO cho admin format progress
/// </summary>
public class AdminProgressRequestDto
{
    public string? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Level { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Response DTO cho admin format progress
/// </summary>
public class ProgressAdminResponseDto
{
    public List<UserProgressDto> UserProgress { get; set; } = new();
    // TEMPORARILY DISABLED due to JSON serialization conflicts
    // public List<ActivityDto> RecentActivities { get; set; } = new();
    public int TotalUsers { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}