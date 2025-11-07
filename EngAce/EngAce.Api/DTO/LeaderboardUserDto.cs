namespace EngAce.Api.DTO
{
    /// <summary>
    /// DTO for Leaderboard User format compatible with frontend useAdminLeaderboard hook
    /// Maps database User data to LeaderboardUser format expected by React Leaderboard component
    /// </summary>
    public class LeaderboardUserDto
    {
        /// <summary>
        /// User's rank position in leaderboard (1-based)
        /// </summary>
        public int Rank { get; set; }
        
        /// <summary>
        /// Username for display - mapped from Name field in database
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Total score points - mapped from TotalXp in database
        /// </summary>
        public int TotalScore { get; set; }
        
        /// <summary>
        /// Listening skill score (TOEIC format: max 495)
        /// </summary>
        public int Listening { get; set; }
        
        /// <summary>
        /// Speaking skill score (TOEIC format: max 200)
        /// </summary>
        public int Speaking { get; set; }
        
        /// <summary>
        /// Reading skill score - mapped from AverageScore for Reading-focused app
        /// </summary>
        public int Reading { get; set; }
        
        /// <summary>
        /// Writing skill score (TOEIC format: max 100)
        /// </summary>
        public int Writing { get; set; }
        
        /// <summary>
        /// Number of completed exams/exercises
        /// </summary>
        public int Exams { get; set; }
        
        /// <summary>
        /// Last activity/update timestamp as ISO string
        /// </summary>
        public string LastUpdate { get; set; } = string.Empty;
    }
}