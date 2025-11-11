using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities
{
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Username { get; set; }

        [MaxLength(255)]
        public string? FullName { get; set; }

        [MaxLength(255)]
        public string? PasswordHash { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Avatar { get; set; }

        [MaxLength(50)]
        public string Role { get; set; } = "user"; // user, admin, moderator

        [MaxLength(50)]
        public string Status { get; set; } = "active"; // active, inactive, suspended

        public bool EmailVerified { get; set; } = false;

        // OAuth fields
        [MaxLength(255)]
        public string? GoogleID { get; set; }

        [MaxLength(255)]
        public string? FacebookID { get; set; }

        // Reset Password
        [MaxLength(255)]
        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }
    }
}
