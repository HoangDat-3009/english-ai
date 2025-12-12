using EngAce.Api.DTO.Core;
using EngAce.Api.DTO.Exercises;
using System.Text.Json.Serialization;

namespace EngAce.Api.DTO.Admin;

// ===== ADMIN-SPECIFIC DTOs =====
// Chỉ chứa DTOs thực sự dành cho admin dashboard, không trùng lặp với Core DTOs

/// <summary>
/// DTO cho admin dashboard - sử dụng DTOs từ Core thay vì định nghĩa lại
/// </summary>
public class AdminDashboardDto
{
    public SystemStatistics Statistics { get; set; } = new();
    public List<TopUserDto> TopUsers { get; set; } = new();
    public List<ActivityDto> RecentActivities { get; set; } = new();
    public ExerciseManagementSummaryDto ExerciseManagement { get; set; } = new();
    public SystemHealthDto SystemHealth { get; set; } = new();
}

/// <summary>
/// DTO cho recent activity trong admin - standalone class để tránh JSON conflicts
/// </summary>
public class RecentActivityDto
{
    // Core activity properties
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Alias for ActivityType
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Details { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string? ExerciseType { get; set; }
    
    // Admin-specific properties
    public string UserName { get; set; } = string.Empty; // Thông tin user cho admin view
    public string Topic { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string AssignmentType { get; set; } = string.Empty;
    public string AdditionalInfo { get; set; } = string.Empty;
    
    // Frontend compatibility
    [JsonPropertyName("date")]
    public string DateString => Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
}

/// <summary>
/// DTO cho system statistics - chỉ dành cho admin
/// </summary>
public class SystemStatistics
{
    [JsonPropertyName("totalUsers")]
    public int TotalUsers { get; set; }
    
    [JsonPropertyName("activeUsers")]
    public int ActiveUsers { get; set; } // Alias for ActiveUsersToday
    public int ActiveUsersToday { get; set; }
    public int ActiveUsersThisWeek { get; set; }
    public int ActiveUsersThisMonth { get; set; }
    
    [JsonPropertyName("totalExercises")]
    public int TotalExercises { get; set; }
    public int CompletedExercises { get; set; } // Alias for CompletedExercisesToday
    public int CompletedExercisesToday { get; set; }
    public int CompletedExercisesThisWeek { get; set; }
    public int CompletedExercisesThisMonth { get; set; }
    public double SystemAverageScore { get; set; }
    
    [JsonPropertyName("averageScore")]
    public double AverageScore { get; set; } // Alias for SystemAverageScore
    public DateTime LastUpdated { get; set; }
    
    // Additional properties used in controllers/services
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
    public double SystemUptime { get; set; }
    public int TotalQuizzes { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalSubmissions { get; set; }
    public int ExercisesCreatedThisWeek { get; set; }
    public int AIGeneratedExercises { get; set; }
    public double AverageCompletionTime { get; set; }
    public int PendingAssignments { get; set; }
}

/// <summary>
/// DTO cho exercise management summary trong admin
/// </summary>
public class ExerciseManagementSummaryDto
{
    public int TotalExercises { get; set; }
    public int PendingApproval { get; set; }
    public int PublishedExercises { get; set; }
    public ExerciseStatsDto Statistics { get; set; } = new();
    public List<ReadingExerciseDto> RecentExercises { get; set; } = new();
}

/// <summary>
/// DTO cho admin settings
/// </summary>
public class AdminSettingsDto
{
    public string SystemName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public bool MaintenanceMode { get; set; }
    public int MaxUsersPerPage { get; set; }
    public int SessionTimeoutMinutes { get; set; }
    public List<string> AllowedFileTypes { get; set; } = new();
}

/// <summary>
/// DTO cho system health check
/// </summary>
public class SystemHealthDto
{
    public bool DatabaseConnected { get; set; }
    public bool DatabaseConnection { get; set; } // Alias for DatabaseConnected
    public bool ExternalApiConnected { get; set; }
    public bool GeminiApiConnection { get; set; } // Alias for ExternalApiConnected
    public double CpuUsage { get; set; }
    public double CpuUsagePercent { get; set; } // Alias for CpuUsage
    public double MemoryUsage { get; set; }
    public double MemoryUsagePercent { get; set; } // Alias for MemoryUsage
    public int ActiveSessions { get; set; }
    public DateTime LastHealthCheck { get; set; }
    public DateTime LastCheckTime { get; set; } // Alias for LastHealthCheck
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    
    // Additional properties used in controllers
    public int ResponseTimeMs { get; set; }
    public string ApplicationVersion { get; set; } = string.Empty;
}