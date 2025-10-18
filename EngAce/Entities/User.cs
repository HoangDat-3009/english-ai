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
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Phone number
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// User role (admin, student, teacher)
        /// </summary>
        public string Role { get; set; } = "student";

        /// <summary>
        /// User status (active, inactive, banned)
        /// </summary>
        public string Status { get; set; } = "active";
    }
}
