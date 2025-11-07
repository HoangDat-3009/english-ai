using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("StudySessions")]
public class StudySession
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    
    public DateTime? EndTime { get; set; }
    
    [Required]
    public int DurationMinutes { get; set; } = 0; // Thời gian học (phút)
    
    [Required]
    [StringLength(50)]
    public string ActivityType { get; set; } = string.Empty; // "reading", "listening", "grammar", "vocabulary"
    
    [StringLength(100)]
    public string? ActivityName { get; set; } // Tên cụ thể của hoạt động
    
    public int ExercisesCompleted { get; set; } = 0; // Số bài tập hoàn thành trong session
    
    [Range(0, 100)]
    public decimal? AverageScore { get; set; } // Điểm trung bình trong session
    
    [Range(0, 1000)]
    public int XPEarned { get; set; } = 0; // XP kiếm được trong session
    
    // Session data (JSON)
    [Column(TypeName = "TEXT")]
    public string? SessionData { get; set; } // Chi tiết về session: exercises, scores, times
    
    [StringLength(500)]
    public string? Notes { get; set; } // Ghi chú từ user
    
    public bool IsCompleted { get; set; } = false;
    
    // Device and location info
    [StringLength(100)]
    public string? DeviceType { get; set; } // "mobile", "desktop", "tablet"
    
    [StringLength(100)]
    public string? Platform { get; set; } // "web", "android", "ios"
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}