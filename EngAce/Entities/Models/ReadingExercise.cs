using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("ReadingExercises")]
public class ReadingExercise
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "TEXT")]
    public string Content { get; set; } = string.Empty; // Nội dung đoạn văn
    
    [Required]
    [StringLength(50)]
    public string Level { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced
    
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = "Part 5"; // Part 5, Part 6, Part 7
    
    [Required]
    [StringLength(50)]
    public string SourceType { get; set; } = "manual"; // uploaded, ai, manual
    
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System"; // Admin username hoặc "AI"
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    [StringLength(500)]
    public string? OriginalFileName { get; set; } // Nếu tạo từ file upload
    
    [StringLength(1000)]
    public string? Description { get; set; } // Mô tả bài tập
    
    public int EstimatedMinutes { get; set; } = 15; // Thời gian dự kiến làm bài
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<ReadingQuestion> Questions { get; set; } = new List<ReadingQuestion>();
    public virtual ICollection<ReadingExerciseResult> Results { get; set; } = new List<ReadingExerciseResult>();
}