using System.ComponentModel.DataAnnotations;

namespace EngAce.Api.DTO
{
    /// <summary>
    /// Request DTO for AI-generated reading exercise creation
    /// Compatible with frontend ReadingExercises page AI generation feature
    /// </summary>
    public class GenerateAIRequest
    {
        /// <summary>
        /// Topic for the reading exercise (e.g., "business meeting", "travel", etc.)
        /// </summary>
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Topic { get; set; } = string.Empty;
        
        /// <summary>
        /// Difficulty level for the exercise
        /// </summary>
        [Required]
        public string Level { get; set; } = "Intermediate"; // Beginner, Intermediate, Advanced
        
        /// <summary>
        /// TOEIC part type for the exercise
        /// </summary>
        [Required] 
        public string Type { get; set; } = "Part 7"; // Part 5, Part 6, Part 7
        
        /// <summary>
        /// Number of questions to generate (default: 5)
        /// </summary>
        [Range(1, 20)]
        public int QuestionCount { get; set; } = 5;
        
        /// <summary>
        /// Creator/admin username (optional)
        /// </summary>
        public string? CreatedBy { get; set; }
        
        /// <summary>
        /// Additional instructions for AI (optional)
        /// </summary>
        [StringLength(500)]
        public string? AdditionalInstructions { get; set; }
    }

    /// <summary>
    /// Response DTO for AI generation status
    /// </summary>
    public class GenerateAIResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ExerciseId { get; set; }
        public string? ErrorDetails { get; set; }
    }


}