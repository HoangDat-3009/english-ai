using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("UserProgresses")]
public class UserProgress
{
    [Key]
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    [Required]
    [Range(0, 495)]
    public int ListeningScore { get; set; } = 0; // Điểm Listening (0-495)
    
    [Required]
    [Range(0, 200)]
    public int SpeakingScore { get; set; } = 0; // Điểm Speaking (0-200)
    
    [Required]
    [Range(0, 495)]
    public int ReadingScore { get; set; } = 0; // Điểm Reading (0-495)
    
    [Required]
    [Range(0, 200)]
    public int WritingScore { get; set; } = 0; // Điểm Writing (0-200)
    
    [Required]
    [Range(0, 990)]
    public int TotalScore { get; set; } = 0; // Tổng điểm TOEIC (0-990)
    
    public int CompletedExercises { get; set; } = 0; // Số bài tập đã hoàn thành
    
    public int TotalExercisesAvailable { get; set; } = 0; // Tổng số bài tập có sẵn
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal AverageAccuracy { get; set; } = 0; // Độ chính xác trung bình (%)
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal ListeningAccuracy { get; set; } = 0; // Độ chính xác Listening
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal ReadingAccuracy { get; set; } = 0; // Độ chính xác Reading
    
    public TimeSpan AverageTimePerExercise { get; set; } // Thời gian trung bình mỗi bài
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    // Weekly progress tracking (JSON serialized)
    [Column(TypeName = "TEXT")]
    public string? WeeklyProgressData { get; set; } // JSON: [{"week": "2023-W42", "score": 750, "exercises": 5}]
    
    // Current streak and goals
    public int CurrentStreak { get; set; } = 0; // Chuỗi ngày học liên tiếp
    
    public int WeeklyGoal { get; set; } = 5; // Mục tiêu bài tập mỗi tuần
    
    public int MonthlyGoal { get; set; } = 20; // Mục tiêu bài tập mỗi tháng
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}