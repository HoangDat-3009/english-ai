using EngAce.Api.DTO.Core;
using EngAce.Api.DTO.Exercises;
using System.ComponentModel.DataAnnotations;

namespace EngAce.Api.DTO.Admin;

// ===== MISSING ADMIN DTOs =====
// Các DTOs còn thiếu cho AdminController

/// <summary>
/// DTO for exercise filter request
/// </summary>
public class ExerciseFilterRequest : PaginationRequestDto
{
    public string? Level { get; set; }
    public string? Type { get; set; }
    public string? SourceType { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public int? MinQuestions { get; set; }
    public int? MaxQuestions { get; set; }
    public string OrderBy { get; set; } = "CreatedAt";
    public bool OrderDescending { get; set; } = true;
}

/// <summary>
/// DTO for paged result
/// </summary>
public class PagedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

/// <summary>
/// DTO for exercise management
/// </summary>
public class ExerciseManagementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int QuestionCount { get; set; }
    public int EstimatedMinutes { get; set; }
    public int TotalAttempts { get; set; }
    public decimal AverageScore { get; set; }
    public TimeSpan AverageCompletionTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
}

/// <summary>
/// DTO for exercise analytics
/// </summary>
public class ExerciseAnalyticsDto
{
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public int UniqueUsers { get; set; }
    public decimal AverageScore { get; set; }
    public decimal MinScore { get; set; }
    public decimal MaxScore { get; set; }
    public TimeSpan AverageCompletionTime { get; set; }
    public List<ScoreDistribution> ScoreDistribution { get; set; } = new();
    public List<DailyAttempts> DailyAttempts { get; set; } = new();
    public List<QuestionAnalytics> QuestionAnalytics { get; set; } = new();
}

/// <summary>
/// DTO for score distribution
/// </summary>
public class ScoreDistribution
{
    public string ScoreRange { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// DTO for daily attempts
/// </summary>
public class DailyAttempts
{
    public DateTime Date { get; set; }
    public int AttemptCount { get; set; }
    public int UniqueUsers { get; set; }
    public decimal AverageScore { get; set; }
}

/// <summary>
/// DTO for question analytics
/// </summary>
public class QuestionAnalytics
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int TotalAnswers { get; set; }
    public int CorrectAnswers { get; set; }
    public decimal CorrectPercentage { get; set; }
    public Dictionary<string, int> AnswerDistribution { get; set; } = new();
}

/// <summary>
/// DTO for bulk operation request
/// </summary>
public class BulkOperationRequest
{
    [Required]
    public List<int> ExerciseIds { get; set; } = new();
    
    [Required]
    public string Operation { get; set; } = string.Empty; // "delete", "activate", "deactivate"
}

/// <summary>
/// Base pagination request DTO
/// </summary>
public class PaginationRequestDto
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public string? SearchTerm { get; set; }
}