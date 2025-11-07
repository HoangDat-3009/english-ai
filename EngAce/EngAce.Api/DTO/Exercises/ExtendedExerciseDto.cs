using System.ComponentModel.DataAnnotations;

namespace EngAce.Api.DTO.Exercises;

// ===== MISSING EXERCISE DTOs =====

/// <summary>
/// DTO for AI generation request
/// </summary>
public class GenerateAIRequest
{
    [Required]
    public string Prompt { get; set; } = string.Empty;
    
    [Required]
    public string Level { get; set; } = string.Empty;
    
    [Required]
    public string Type { get; set; } = string.Empty;
    
    public int QuestionCount { get; set; } = 5;
    public string? Context { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty; // Property used in controllers
}

/// <summary>
/// DTO for AI generation response
/// </summary>
public class GenerateAIResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ReadingExerciseDto? Exercise { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    // Additional properties used in controllers
    public string Status { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public int ExerciseId { get; set; }
    public string ErrorDetails { get; set; } = string.Empty;
}

/// <summary>
/// DTO for create user request
/// </summary>
public class CreateUserRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    public string UserName { get; set; } = string.Empty; // Alias for Username
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string FullName { get; set; } = string.Empty;
    
    public string Level { get; set; } = "Beginner";
    public string PreferredLevel { get; set; } = "Beginner"; // Alias for Level
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "Active";
}

/// <summary>
/// DTO for recent exercise information
/// </summary>
public class RecentExerciseDto
{
    public int Id { get; set; }
    public int ExerciseId { get; set; } // Alias for Id
    public string Name { get; set; } = string.Empty;
    public string ExerciseName { get; set; } = string.Empty; // Alias for Name
    public string Title { get; set; } = string.Empty; // Added for compatibility
    public string Type { get; set; } = string.Empty;
    public string ExerciseType { get; set; } = string.Empty; // Alias for Type
    public string Level { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
    public int Score { get; set; }
    public TimeSpan TimeSpent { get; set; }
    public TimeSpan Duration { get; set; } // Alias for TimeSpent
}

/// <summary>
/// DTO for exercise type performance
/// </summary>
public class ExerciseTypePerformanceDto
{
    public string ExerciseType { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Alias for ExerciseType
    public int TotalAttempts { get; set; }
    public double AverageScore { get; set; }
    public double BestScore { get; set; }
    public TimeSpan AverageTime { get; set; }
    public TimeSpan TotalTimeSpent { get; set; } // Additional property
    public DateTime LastAttempt { get; set; }
}

/// <summary>
/// DTO for update user request
/// </summary>
public class UpdateUserRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string FullName { get; set; } = string.Empty;
    
    public string Level { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    
    // Additional properties used in controllers
    public string PreferredLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalXP { get; set; }
}