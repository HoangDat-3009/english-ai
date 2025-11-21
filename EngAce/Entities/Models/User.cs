using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255)]
    [Column("password_hash")]
    public string Password { get; set; } = string.Empty; // Stored hash
    
    [Required]
    [StringLength(100)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(255)]
    [Column("google_id")]
    public string? GoogleId { get; set; }
    
    [StringLength(255)]
    [Column("facebook_id")]
    public string? FacebookId { get; set; }
    
    [StringLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }
    
    [Column("full_name", TypeName = "NVARCHAR(100)")]
    public string? FullName { get; set; }
    
    [Column("bio", TypeName = "TEXT")]
    public string? Bio { get; set; }
    
    [Column("address", TypeName = "NVARCHAR(255)")]
    public string? Address { get; set; }
    
    [Required]
    [StringLength(20)]
    [Column("status")]
    public string Status { get; set; } = "active"; // ENUM('active','inactive','banned')
    
    [Required]
    [StringLength(20)]
    [Column("account_type")]
    public string AccountType { get; set; } = "free"; // ENUM('free','premium')
    
    [Required]
    [StringLength(20)]
    [Column("role")]
    public string Role { get; set; } = "user"; // ENUM('user','admin')
    
    [Column("premium_expires_at")]
    public DateTime? PremiumExpiresAt { get; set; }
    
    [Column("total_study_time")]
    public int TotalStudyTime { get; set; } = 0;
    
    [Column("total_xp")]
    public int TotalXp { get; set; } = 0;
    
    [StringLength(255)]
    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }
    
    [Column("last_active_at")]
    public DateTime? LastActiveAt { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public bool IsActive
    {
        get => string.Equals(Status, "active", StringComparison.OrdinalIgnoreCase);
        set => Status = value ? "active" : "inactive";
    }
    
    [NotMapped]
    public string? Avatar => AvatarUrl;
    
    // Navigation properties
    public virtual ICollection<Completion> Completions { get; set; } = new List<Completion>();
}
