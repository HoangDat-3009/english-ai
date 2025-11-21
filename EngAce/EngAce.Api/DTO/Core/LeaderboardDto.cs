using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EngAce.Api.DTO.Core;

// ===== STANDARDIZED LEADERBOARD DTOs =====
// Hợp nhất từ LeaderboardDto.cs và UserManagementDto.cs để loại bỏ duplicate

/// <summary>
/// DTO chuẩn hóa cho leaderboard entry - hợp nhất từ 2 LeaderboardEntryDto khác nhau
/// </summary>
public class LeaderboardEntryDto
{
    [JsonPropertyName("userId")]
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    
    // Scoring information
    public int TotalScore { get; set; }
    public int TotalXP { get; set; }
    public int FilteredXP { get; set; }
    public int CompletedExercises { get; set; }
    public int FilteredExercises { get; set; }
    public int TotalExercises { get; set; }
    public double AverageScore { get; set; }
    public decimal AverageAccuracy { get; set; }
    
    // Skills scores
    public int ListeningScore { get; set; }
    public int SpeakingScore { get; set; }
    public int ReadingScore { get; set; }
    public int WritingScore { get; set; }

    [JsonPropertyName("toeicParts")]
    public List<ToeicPartScoreDto> ToeicParts { get; set; } = new();
    
    // Ranking and activity
    public int Rank { get; set; }
    public DateTime LastActivity { get; set; }
    public DateTime LastActive { get; set; } // Alias for LastActivity
    public int StudyStreak { get; set; }
    public int StreakDays { get; set; }
    public string Badge { get; set; } = string.Empty;
}



/// <summary>
/// DTO cho leaderboard statistics - từ LeaderboardDto.cs
/// </summary>
public class LeaderboardStatsDto
{
    public int TotalParticipants { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsersToday { get; set; }
    public double AverageScore { get; set; }
    public int HighestScore { get; set; }
    public int TotalExercisesCompleted { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// DTO cho leaderboard response - từ UserManagementDto.cs
/// </summary>
public class LeaderboardResponseDto
{
    public List<LeaderboardEntryDto> Entries { get; set; } = new();
    public int TotalUsers { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public LeaderboardStatsDto Stats { get; set; } = new();
    
    // Additional properties used in code
    public string TimeFilter { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO cho filtered leaderboard request
/// </summary>
public class FilteredLeaderboardRequestDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? Level { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}