using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    /// <summary>
    /// Entity ánh xạ bảng exercises trong cơ sở dữ liệu hiện tại (english_mentor_buddy_db).
    /// </summary>
    [Table("exercises")]
    public class Exercise
    {
        [Key]
        [Column("id")]
        public int ExerciseId { get; set; }

        [Required]
        [Column("title", TypeName = "NVARCHAR(200)")]
        public string Title { get; set; } = string.Empty;

        [Column("content", TypeName = "TEXT")]
        public string? Content { get; set; }

        // JSON string chứa danh sách câu hỏi
        [Required]
        [Column("questions_json", TypeName = "JSON")]
        public string Questions { get; set; } = "[]";

        [Required]
        [Column("correct_answers_json", TypeName = "JSON")]
        public string CorrectAnswers { get; set; } = "[]";

        [StringLength(50)]
        [Column("level")]
        public string? Level { get; set; }

        [StringLength(50)]
        [Column("type")]
        public string? Type { get; set; }

        [StringLength(50)]
        [Column("category")]
        public string? Category { get; set; }

        [Column("estimated_minutes")]
        public int? EstimatedMinutes { get; set; } = 30;

        [Column("time_limit")]
        public int? TimeLimit { get; set; }

        [StringLength(255)]
        [Column("audio_url")]
        public string? AudioUrl { get; set; }

        [StringLength(50)]
        [Column("source_type")]
        public string? SourceType { get; set; }

        [Column("created_by")]
        public int? CreatedByUserId { get; set; }

        [ForeignKey(nameof(CreatedByUserId))]
        public virtual User? CreatedByUser { get; set; }

        [NotMapped]
        public string CreatedByDisplay =>
            CreatedByUser?.FullName
            ?? CreatedByUser?.Username
            ?? (SourceType == "ai" ? "AI System" : "System");

        [Column("original_file_name", TypeName = "NVARCHAR(255)")]
        public string? OriginalFileName { get; set; }

        [Column("description", TypeName = "NVARCHAR(500)")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Note: updated_at có ON UPDATE CURRENT_TIMESTAMP trong database

        // Navigation properties
        public virtual ICollection<Completion> Completions { get; set; } = new List<Completion>();
    }
}
