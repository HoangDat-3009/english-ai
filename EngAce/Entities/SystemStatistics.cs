namespace Entities
{
    /// <summary>
    /// Represents system-wide statistics
    /// </summary>
    public class SystemStatistics
    {
        /// <summary>
        /// Total number of users in the system (students only)
        /// Total number of users in the system
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// Number of active users (status = 'active')
        /// </summary>
        public int ActiveUsers { get; set; }

        /// <summary>
        /// Number of new users registered this month
        /// </summary>
        public int NewUsersThisMonth { get; set; }

        /// <summary>
        /// Total number of tests (exams) in the system
        /// </summary>
        public int TotalTests { get; set; }

        /// <summary>
        /// Total number of exercises in the system
        /// </summary>
        public int TotalExercises { get; set; }

        /// <summary>
        /// Total number of test completions
        /// </summary>
        public int TotalCompletions { get; set; }

        /// <summary>
        /// Total revenue from completed payments (in VND)
        /// </summary>
        public double TotalRevenue { get; set; }

        /// <summary>
        /// Revenue from completed payments this month (in VND)
        /// </summary>
        public double RevenueThisMonth { get; set; }

        /// <summary>
        /// Number of pending payments
        /// </summary>
        public int PendingPayments { get; set; }
    }
}
