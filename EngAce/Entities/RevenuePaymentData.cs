namespace Entities
{
    /// <summary>
    /// Represents revenue and payment data for a specific month
    /// </summary>
    public class RevenuePaymentData
    {
        /// <summary>
        /// Month label (e.g., "T1", "T2", etc.)
        /// </summary>
        public string Month { get; set; } = string.Empty;

        /// <summary>
        /// Total revenue from completed payments (in VND)
        /// </summary>
        public double Revenue { get; set; }

        /// <summary>
        /// Total number of payments in this month
        /// </summary>
        public int TotalPayments { get; set; }

        /// <summary>
        /// Total amount from pending payments (in VND)
        /// </summary>
        public double PendingAmount { get; set; }

        /// <summary>
        /// Total amount from failed payments (in VND)
        /// </summary>
        public double FailedAmount { get; set; }
    }
}
