using System.ComponentModel.DataAnnotations;

namespace EngAce.Api.DTO
{
    /// <summary>
    /// DTO for Admin User data format compatible with frontend useCurrentUserProgress hook
    /// Maps database User entity to AdminUser format expected by React components
    /// </summary>
    public class AdminUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Total XP points - mapped from TotalScore in database
        /// </summary>
        public int TotalXp { get; set; }
        
        /// <summary>
        /// User level as integer - converted from Level string in database
        /// </summary>
        public int Level { get; set; }
        
        /// <summary>
        /// Average score across all completed exercises
        /// </summary>
        public decimal AverageScore { get; set; }
        
        /// <summary>
        /// Number of reading exercises completed
        /// </summary>
        public int ExercisesCompleted { get; set; }
        
        /// <summary>
        /// Current study streak in days
        /// </summary>
        public int StreakDays { get; set; }
        
        /// <summary>
        /// List of achievement names/badges earned
        /// </summary>
        public List<string> Achievements { get; set; } = new();
        
        /// <summary>
        /// Last activity timestamp
        /// </summary>
        public DateTime LastActive { get; set; }
        
        /// <summary>
        /// Account creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}