using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    /// <summary>
    /// Ánh xạ bảng exercise_completions trong cơ sở dữ liệu hiện tại.
    /// </summary>
    [Table("exercise_completions")]
    public class Completion
    {
        [Key]
        [Column("id")]
        public int CompletionId { get; set; }

        [Required]
        [ForeignKey("User")]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [ForeignKey("Exercise")]
        [Column("exercise_id")]
        public int ExerciseId { get; set; }

        [Column("user_answers_json", TypeName = "JSON")]
        public string? UserAnswers { get; set; }

        [Column("score", TypeName = "decimal(5,2)")]
        public decimal? Score { get; set; }

        [Column("total_questions")]
        public int? TotalQuestions { get; set; }

        [Column("started_at")]
        public DateTime? StartedAt { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [Column("is_completed")]
        public bool IsCompleted { get; set; } = false;

        [Column("time_spent_minutes")]
        public int? TimeSpentMinutes { get; set; }

        [Column("attempts")]
        public int Attempts { get; set; } = 1;

        [Column("ai_graded")]
        public bool AiGraded { get; set; } = false;

        [Column("review_status")]
        public string? ReviewStatus { get; set; } = "pending";

        [Column("reviewed_by")]
        public int? ReviewedBy { get; set; }

        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        [Column("original_score", TypeName = "decimal(5,2)")]
        public decimal? OriginalScore { get; set; }

        [Column("final_score", TypeName = "decimal(5,2)")]
        public decimal? FinalScore { get; set; }

        [Column("review_notes")]
        public string? ReviewNotes { get; set; }

        [Column("confidence_score", TypeName = "decimal(3,2)")]
        public decimal? ConfidenceScore { get; set; }
        
        // Note: UNIQUE constraint trên (user_id, exercise_id, attempts) trong database

        [NotMapped]
        public TimeSpan? TimeSpent
        {
            get => TimeSpentMinutes.HasValue ? TimeSpan.FromMinutes(TimeSpentMinutes.Value) : null;
            set => TimeSpentMinutes = value?.TotalMinutes > 0 ? (int)value.Value.TotalMinutes : null;
        }

        public virtual User User { get; set; } = null!;
        public virtual Exercise Exercise { get; set; } = null!;
        public virtual ICollection<CompletionScore> CompletionScores { get; set; } = new List<CompletionScore>();
    }
}
