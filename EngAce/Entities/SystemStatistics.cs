namespace Entities
{
    /// <summary>
    /// Represents system-wide statistics
    /// </summary>
    public class SystemStatistics
    {
        /// <summary>
        /// Total number of users in the system
        /// </summary>
        public int TotalUsers { get; set; }

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
    }
}
