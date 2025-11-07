using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("ReadingExerciseResults")]
public class ReadingExerciseResult
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    [Required]
    [ForeignKey("ReadingExercise")]
    public int ReadingExerciseId { get; set; }
    
    [Required]
    [Range(0, 100)]
    public int Score { get; set; } // Điểm số (8/10 = 80)
    
    [Required]
    [Range(0, 100)]
    public int TotalQuestions { get; set; } // Tổng số câu hỏi
    
    [Required]
    [Range(0, 100)]
    public int CorrectAnswers { get; set; } // Số câu trả lời đúng
    
    [Required]
    [Column(TypeName = "TEXT")]
    public string UserAnswers { get; set; } = string.Empty; // "[0,1,2,1,3,0,2,1,0,3]" (JSON array)
    
    [Required]
    public TimeSpan TimeSpent { get; set; } // Thời gian làm bài
    
    [Required]
    public DateTime StartedAt { get; set; }
    
    [Required]
    public DateTime CompletedAt { get; set; }
    
    [Range(1, 5)]
    public int? DifficultyRating { get; set; } // User feedback về độ khó
    
    [StringLength(1000)]
    public string? UserFeedback { get; set; } // Phản hồi từ user
    
    public bool IsCompleted { get; set; } = true;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ReadingExercise ReadingExercise { get; set; } = null!;
}