namespace Entities
{
    /// <summary>
    /// Represents user growth data for a specific month
    /// </summary>
    public class UserGrowthData
    {
        /// <summary>
        /// Month label (e.g., "T1", "T2", etc.)
        /// </summary>
        public string Month { get; set; } = string.Empty;

        /// <summary>
        /// Number of new users registered in this month
        /// </summary>
        public int NewUsers { get; set; }

        /// <summary>
        /// Number of new users who are still active
        /// </summary>
        public int ActiveUsers { get; set; }
    }
}
