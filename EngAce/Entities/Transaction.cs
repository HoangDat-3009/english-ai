namespace Entities
{
    /// <summary>
    /// Represents a payment transaction in the system
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Transaction ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User ID who made the payment
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Package ID purchased
        /// </summary>
        public int PackageId { get; set; }

        /// <summary>
        /// Payment amount in VND
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Payment method (e.g., momo, bank, paypal, credit)
        /// </summary>
        public string? Method { get; set; }

        /// <summary>
        /// Payment status (pending, completed, failed)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is a lifetime purchase
        /// </summary>
        public bool IsLifetime { get; set; }

        /// <summary>
        /// Transaction history notes
        /// </summary>
        public string? TransactionHistory { get; set; }

        /// <summary>
        /// Transaction creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
