namespace Entities
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class User
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Password hash (bcrypt)
        /// </summary>
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Google ID (sub)
        /// </summary>
        public string? GoogleId { get; set; }

        /// <summary>
        /// Facebook ID
        /// </summary>
        public string? FacebookId { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Full name
        /// </summary>
        public string? FullName { get; set; }

        /// <summary>
        /// Biography
        /// </summary>
        public string? Bio { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// User status (active, inactive, banned)
        /// </summary>
        public string Status { get; set; } = "active";

        /// <summary>
        /// Account type (free, premium)
        /// </summary>
        public string AccountType { get; set; } = "free";

        /// <summary>
        /// Premium expiration date (NULL = lifetime premium)
        /// </summary>
        public DateTime? PremiumExpiresAt { get; set; }

        /// <summary>
        /// Total study time in minutes
        /// </summary>
        public int TotalStudyTime { get; set; } = 0;

        /// <summary>
        /// Total XP points
        /// </summary>
        public int TotalXP { get; set; } = 0;

        /// <summary>
        /// Avatar URL
        /// </summary>
        public string? Avatar { get; set; }

        /// <summary>
        /// User role (user, admin, super_admin)
        /// </summary>
        public string Role { get; set; } = "user";

        /// <summary>
        /// Last active timestamp
        /// </summary>
        public DateTime? LastActiveAt { get; set; }

        /// <summary>
        /// Last login timestamp
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Created timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Updated timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
