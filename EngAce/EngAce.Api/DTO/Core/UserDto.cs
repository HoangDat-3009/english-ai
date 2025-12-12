using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EngAce.Api.DTO.Core;

// ===== STANDARDIZED USER DTOs =====
// Hợp nhất từ AdminDto.cs và UserManagementDto.cs

/// <summary>
/// DTO chi tiết user - hợp nhất từ UserDetailDto và UserManagementDto
/// </summary>
public class UserDetailDto
{
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("userName")]
    public string Username { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    [JsonPropertyName("lastLoginAt")]
    public DateTime LastLoginAt { get; set; } // Alias for LastLoginDate
    public DateTime LastLoginDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    // XP and scoring
    public int TotalXP { get; set; }
    public int WeeklyXP { get; set; }
    public int MonthlyXP { get; set; }
    public int TodayXP { get; set; }
    
    // Analytics data
    public int TotalExercisesCompleted { get; set; }
    public double OverallAverageScore { get; set; }
    [JsonPropertyName("avgScore")]
    public double AverageScore { get; set; } // Alias for OverallAverageScore
    public double BestScore { get; set; }
    public TimeSpan TotalStudyTime { get; set; }
    public int StreakDays { get; set; }
    
    // Progress information
    public int CurrentRank { get; set; }
    public int TotalScore { get; set; }
    public string PreferredLevel { get; set; } = string.Empty;
    
    // Additional info
    public List<string> Achievements { get; set; } = new();
    public List<object> RecentExercises { get; set; } = new(); // Will be typed later
    public List<object> PerformanceByType { get; set; } = new(); // Will be typed later
}

/// <summary>
/// DTO cho user analytics - từ UserManagementDto.cs
/// </summary>
public class UserAnalyticsDto
{
    public string UserId { get; set; } = string.Empty;
    public int TotalExercisesCompleted { get; set; }
    public double OverallAverageScore { get; set; }
    public TimeSpan TotalStudyTime { get; set; }
    public DateTime LastLoginDate { get; set; }
    public int StreakDays { get; set; }
    public List<DailyActivityDto> RecentActivity { get; set; } = new();
}

/// <summary>
/// DTO cho daily activity - từ UserAnalyticsDto
/// </summary>
public class DailyActivityDto
{
    public DateTime Date { get; set; }
    public int ExercisesCompleted { get; set; }
    public TimeSpan StudyTime { get; set; }
    public double AverageScore { get; set; }
}

/// <summary>
/// DTO for top performing users in admin dashboard  
/// </summary>
public class TopUserDto
{
    public string UserId { get; set; } = string.Empty;
    
    [JsonPropertyName("userName")]
    public string Username { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("totalScore")]
    public int TotalScore { get; set; }
    
    [JsonPropertyName("completedExercises")]
    public int CompletedExercises { get; set; }
    
    public int TotalExercises { get; set; }
    
    [JsonPropertyName("averageScore")]
    public double AverageScore { get; set; }
    
    public int TotalXP { get; set; }
    public int WeeklyXP { get; set; }
    
    [JsonPropertyName("lastActive")]
    public DateTime LastActive { get; set; }
    
    public DateTime LastActivity { get; set; }
    
    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;
    
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO cho user management list - hợp nhất từ AdminDto.cs
/// </summary>
public class UserManagementDto
{
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("userName")]
    public string Username { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginDate { get; set; }
    [JsonPropertyName("lastLoginAt")]
    public DateTime LastLoginAt { get; set; } // Alias for LastLoginDate
    public string Status { get; set; } = string.Empty;
    
    // XP and scoring
    public int TotalXP { get; set; }
    public int WeeklyXP { get; set; }
    public int MonthlyXP { get; set; }
    
    // Exercises
    public int TotalExercises { get; set; }
    public int TotalExercisesCompleted { get; set; }
    public double AverageScore { get; set; }
    public int StreakDays { get; set; }
    public string PreferredLevel { get; set; } = string.Empty;
    public List<string> Achievements { get; set; } = new();
}

/// <summary>
/// DTO cho import result - từ UserManagementDto.cs
/// </summary>
public class ImportResultDto
{
    public int TotalProcessed { get; set; }
    [JsonPropertyName("totalRows")]
    public int TotalRows { get; set; } // Alias for TotalProcessed
    public int SuccessfulImports { get; set; }
    [JsonPropertyName("successCount")]
    public int SuccessCount { get; set; } // Alias for SuccessfulImports
    public int FailedImports { get; set; }
    [JsonPropertyName("errorCount")]
    public int ErrorCount { get; set; } // Alias for FailedImports
    public List<string> Errors { get; set; } = new();
    public DateTime ProcessedAt { get; set; }
}

/// <summary>
/// DTO cho user progress summary - từ UserManagementDto.cs
/// </summary>
public class UserProgressSummaryDto
{
    public string UserId { get; set; } = string.Empty;
    public int CompletedExercises { get; set; }
    public double AverageScore { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime LastActivity { get; set; }
    public string CurrentLevel { get; set; } = string.Empty;
    public double ProgressToNextLevel { get; set; }
}