using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EngAce.Api.DTO.Exercises;

// ===== READING EXERCISE DTOs =====
public class ReadingExerciseDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty; // Alias for Name
    public string Content { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty; // "Beginner", "Intermediate", "Advanced"
    public string Type { get; set; } = string.Empty; // "Part 5", "Part 6", "Part 7"
    public string SourceType { get; set; } = "manual"; // "uploaded", "ai", "manual"
    public string? Description { get; set; }
    public int EstimatedMinutes { get; set; }
    public int Duration { get; set; } // Alias for EstimatedMinutes
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime DateCreated { get; set; } // Alias for CreatedAt
    public List<QuestionDto> Questions { get; set; } = new();
    public UserResultDto? UserResult { get; set; }
    
    // === FRONTEND COMPATIBILITY PROPERTIES ===
    // Match ReadingExercise interface from frontend    
    [JsonPropertyName("source_type")]
    public string SourceTypeSnakeCase => SourceType; // Match frontend snake_case
    
    [JsonPropertyName("created_at")]
    public string CreatedAtIsoString => CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"); // ISO format
    
    // Additional properties used in services
    public string Status { get; set; } = "Active";
    public int AttemptCount { get; set; }
    public double AverageScore { get; set; }
    public bool IsPublished { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

public class QuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty; // Alias for QuestionText
    public List<string> Options { get; set; } = new();
    public List<string> Choices { get; set; } = new(); // Alias for Options
    
    [JsonPropertyName("correctAnswer")]
    public int CorrectAnswer { get; set; }
    public int Answer { get; set; } // Alias for CorrectAnswer
    public string? Explanation { get; set; }
    public int Difficulty { get; set; } // 1-5
    
    // === FRONTEND COMPATIBILITY PROPERTIES ===
    // Match Question interface from frontend
    [JsonPropertyName("question")]
    public string QuestionAlias => QuestionText; // Frontend expects 'question' not 'QuestionText'
    
    // Additional properties
    public string Type { get; set; } = string.Empty;
    public int Points { get; set; }
    public TimeSpan TimeLimit { get; set; }
}

public class UserResultDto
{
    public int Score { get; set; }
    public int Points { get; set; } // Alias for Score
    public int CorrectAnswers { get; set; }
    public int Correct { get; set; } // Alias for CorrectAnswers
    [JsonPropertyName("total")]
    public int TotalQuestions { get; set; }
    [JsonPropertyName("totalCount")]
    public int Total { get; set; } // Alias for TotalQuestions
    public DateTime CompletedAt { get; set; }
    public DateTime SubmittedAt { get; set; } // Alias for CompletedAt
    
    [JsonPropertyName("submitted_at")]
    public string SubmittedAtIsoString => SubmittedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    
    // Additional properties
    public int TimeSpent { get; set; }
    public double Percentage { get; set; }
    public string Grade { get; set; } = string.Empty;
    public List<int> UserAnswers { get; set; } = new();
    public bool Passed { get; set; }
}

public class CreateExerciseDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string Level { get; set; } = string.Empty; // "Beginner", "Intermediate", "Advanced"

    [Required]
    public string Type { get; set; } = string.Empty; // "Part 5", "Part 6", "Part 7"

    public string? Description { get; set; }

    [Range(1, 120)]
    public int EstimatedMinutes { get; set; }

    public string? CreatedBy { get; set; }
}

public class UpdateExerciseDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string Level { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(1, 120)]
    public int EstimatedMinutes { get; set; }
}

public class SubmitExerciseDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public List<int> Answers { get; set; } = new();
}

public class CreateExerciseWithAIRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public string Level { get; set; } = string.Empty; // "Beginner", "Intermediate", "Advanced"
    
    [Required]
    public string Type { get; set; } = string.Empty; // "Part 5", "Part 6", "Part 7"
    
    public string? Description { get; set; }
    
    public int EstimatedMinutes { get; set; } = 15;
    
    [Required]
    public string CreatedBy { get; set; } = string.Empty;
    
    public int? QuestionCount { get; set; } = 5;
}

public class SubmitExerciseResultRequest
{
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public int ExerciseId { get; set; }
    
    [Required]
    public List<int> Answers { get; set; } = new();
    
    public int? TimeSpent { get; set; }
    
    public string CompletedAt { get; set; } = DateTime.UtcNow.ToString("O");
}

// ===== ADDITIONAL EXERCISE DTOs FOR ADMIN =====

/// <summary>
/// DTO for exercise statistics
/// </summary>
public class ExerciseStatsDto
{
    public int TotalExercises { get; set; }
    public int TotalAttempts { get; set; }
    public double AverageScore { get; set; }
    public double CompletionRate { get; set; }
    public List<LevelStatsDto> LevelStats { get; set; } = new();
}

/// <summary>
/// DTO for level statistics
/// </summary>
public class LevelStatsDto
{
    public string Level { get; set; } = string.Empty;
    public int ExerciseCount { get; set; }
    public int AttemptCount { get; set; }
    public double AverageScore { get; set; }
}

/// <summary>
/// DTO for file upload process results
/// </summary>
public class FileUploadResultDto
{
    public int TotalUploaded { get; set; }
    public int SuccessfulUploads { get; set; }
    public int FailedUploads { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
    public DateTime UploadedAt { get; set; }
}