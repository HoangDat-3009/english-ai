using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    /// <summary>
    /// Detailed scoring for each question in an exercise completion.
    /// </summary>
    [Table("exercise_completion_scores")]
    public class CompletionScore
    {
        [Key]
        [Column("id")]
        public int CompletionScoreId { get; set; }

        [Required]
        [ForeignKey("Completion")]
        [Column("completion_id")]
        public int CompletionId { get; set; }

        [Required]
        [Column("question_number")]
        public int QuestionNumber { get; set; }

        [Column("user_answer", TypeName = "TEXT")]
        public string? UserAnswer { get; set; }

        [Column("correct_answer", TypeName = "TEXT")]
        public string? CorrectAnswer { get; set; }

        [Column("is_correct")]
        public bool? IsCorrect { get; set; }

        [Column("points", TypeName = "decimal(5,2)")]
        public decimal? Points { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Completion Completion { get; set; } = null!;
    }
}