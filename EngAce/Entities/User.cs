using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [Column("email")]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Column("username")]
        [MaxLength(100)]
        public string? Username { get; set; }

        [Column("full_name")]
        [MaxLength(255)]
        public string? FullName { get; set; }

        [Column("password_hash")]
        [MaxLength(255)]
        public string? PasswordHash { get; set; }

        [Column("phone")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("avatar_url")]
        [MaxLength(500)]
        public string? Avatar { get; set; }

        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; } = "active"; // active, inactive, banned

        [Column("account_type")]
        [MaxLength(50)]
        public string? AccountType { get; set; } = "free"; // free, premium

        [Column("role")]
        [MaxLength(50)]
        public string UserRole { get; set; } = "customer"; // admin, customer

        // OAuth fields
        [Column("google_id")]
        [MaxLength(255)]
        public string? GoogleID { get; set; }

        [Column("facebook_id")]
        [MaxLength(255)]
        public string? FacebookID { get; set; }

        // Timestamps
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_active_at")]
        public DateTime? LastLoginAt { get; set; }

        // Additional fields from database
        [Column("bio")]
        public string? Bio { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("premium_expires_at")]
        public DateTime? PremiumExpiresAt { get; set; }

        [Column("total_study_time")]
        public int TotalStudyTime { get; set; } = 0;

        [Column("total_xp")]
        public int TotalXP { get; set; } = 0;

        // For backward compatibility with existing code
        [NotMapped]
        public string Role => UserRole ?? "customer";

        [NotMapped]
        public bool EmailVerified => true;

        [NotMapped]
        public string? ResetToken { get; set; }

        [NotMapped]
        public DateTime? ResetTokenExpires { get; set; }
    }
}
