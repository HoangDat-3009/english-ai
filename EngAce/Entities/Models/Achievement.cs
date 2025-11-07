using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("Achievements")]
public class Achievement
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty; // "First Perfect Score", "Speed Demon"
    
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty; // Mô tả thành tích
    
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty; // "score", "streak", "completion", "speed"
    
    [StringLength(100)]
    public string Icon { get; set; } = "🏆"; // Icon/emoji cho achievement
    
    [StringLength(50)]
    public string Rarity { get; set; } = "Common"; // Common, Rare, Epic, Legendary
    
    [Range(0, 1000)]
    public int Points { get; set; } = 0; // XP points được từ achievement
    
    [Required]
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
    
    // Criteria for achievement (JSON)
    [Column(TypeName = "TEXT")]
    public string? Criteria { get; set; } // {"type": "score", "target": 100, "skill": "reading"}
    
    [Column(TypeName = "TEXT")]
    public string? Metadata { get; set; } // Additional data về achievement
    
    public bool IsVisible { get; set; } = true; // Hiển thị trong profile không
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}