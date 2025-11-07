using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string Level { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced
    
    public int StudyStreak { get; set; } = 0; // Chuỗi học tập (ngày)
    
    public int TotalStudyTime { get; set; } = 0; // Tổng thời gian học (phút)
    
    public int TotalXP { get; set; } = 0; // Experience points
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
    
    [StringLength(500)]
    public string? Avatar { get; set; } // URL to avatar image
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<ReadingExerciseResult> ExerciseResults { get; set; } = new List<ReadingExerciseResult>();
    public virtual ICollection<StudySession> StudySessions { get; set; } = new List<StudySession>();
    public virtual ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
    public virtual UserProgress? Progress { get; set; }
}