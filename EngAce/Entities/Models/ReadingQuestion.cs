using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("ReadingQuestions")]
public class ReadingQuestion
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("ReadingExercise")]
    public int ReadingExerciseId { get; set; }
    
    [Required]
    [Column(TypeName = "TEXT")]
    public string QuestionText { get; set; } = string.Empty; // "The company will _____ a new product."
    
    [Required]
    [StringLength(500)]
    public string OptionA { get; set; } = string.Empty; // "launch"
    
    [Required]
    [StringLength(500)]
    public string OptionB { get; set; } = string.Empty; // "launches"
    
    [Required]
    [StringLength(500)]
    public string OptionC { get; set; } = string.Empty; // "launching"
    
    [Required]
    [StringLength(500)]
    public string OptionD { get; set; } = string.Empty; // "launched"
    
    [Required]
    [Range(0, 3)]
    public int CorrectAnswer { get; set; } // 0=A, 1=B, 2=C, 3=D
    
    [Column(TypeName = "TEXT")]
    public string? Explanation { get; set; } // "Future tense cần động từ nguyên mẫu"
    
    public int OrderNumber { get; set; } = 1; // Thứ tự câu hỏi trong bài
    
    [Range(1, 5)]
    public int Difficulty { get; set; } = 3; // 1=Very Easy, 5=Very Hard
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ReadingExercise ReadingExercise { get; set; } = null!;
}