using EngAce.Api.DTO.Core;
using System.ComponentModel.DataAnnotations;

namespace EngAce.Api.DTO.Core;

// ===== MISSING LEADERBOARD DTOs =====

/// <summary>
/// DTO for admin leaderboard user - extends LeaderboardEntryDto
/// </summary>
public class LeaderboardUserDto : LeaderboardEntryDto
{
    public new string FullName { get; set; } = string.Empty;
    public new string Level { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public new int StreakDays { get; set; }
    public TimeSpan TotalStudyTime { get; set; }
    
    // Skill scores properties used in controllers
    public int Listening { get; set; }
    public int Speaking { get; set; }
    public int Reading { get; set; }
    public int Writing { get; set; }
    public int Exams { get; set; }
    public DateTime LastUpdate { get; set; }
}

/// <summary>
/// DTO for user rank information
/// </summary>
public class UserRankDto
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int Rank { get; set; }
    public int CurrentRank { get; set; } // Alias for Rank
    public int TotalUsers { get; set; }
    public int TotalXP { get; set; }
    public double Percentile { get; set; }
    public string RankCategory { get; set; } = string.Empty;
}

/// <summary>
/// DTO for admin user format
/// </summary>
public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public int TotalExercises { get; set; }
    public double AverageScore { get; set; }
    public int TotalScore { get; set; }
    public int CurrentRank { get; set; }
    
    // Additional properties used in controllers
    public int TotalXp { get; set; } // Alias for TotalScore
    public int ExercisesCompleted { get; set; }
    public int StreakDays { get; set; }
    public List<string> Achievements { get; set; } = new();
    public DateTime LastActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}



/// <summary>
/// Extended progress response for admin format
/// </summary>
public class AdminExtendedProgressResponseDto
{
    public List<AdminUserDto> Users { get; set; } = new();
    public List<ActivityDto> RecentActivities { get; set; } = new();
    public List<WeeklyProgressDto> WeeklyProgress { get; set; } = new();
    public int TotalUsers { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}