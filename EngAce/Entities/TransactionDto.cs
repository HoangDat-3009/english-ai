namespace Entities
{
    /// <summary>
    /// Data transfer object for transaction information
    /// </summary>
    public class TransactionDto
    {
        /// <summary>
        /// Transaction ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// User ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// User's full name
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// User's email address
        /// </summary>
        public string UserEmail { get; set; } = string.Empty;

        /// <summary>
        /// Payment amount in VND
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Payment status (completed, pending, failed)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Transaction creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Transaction last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Detailed transaction information including payment method and notes
    /// </summary>
    public class TransactionDetailDto : TransactionDto
    {
        /// <summary>
        /// Payment method (e.g., momo, bank, paypal, credit)
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Transaction notes or history
        /// </summary>
        public string? TransactionNotes { get; set; }

        /// <summary>
        /// Package ID purchased
        /// </summary>
        public string? PackageId { get; set; }

        /// <summary>
        /// Whether this is a lifetime purchase
        /// </summary>
        public bool IsLifetime { get; set; }
    }

    /// <summary>
    /// Summary statistics for a set of transactions
    /// </summary>
    public class TransactionSummary
    {
        /// <summary>
        /// Total revenue from completed transactions
        /// </summary>
        public double TotalRevenue { get; set; }

        /// <summary>
        /// Total number of transactions
        /// </summary>
        public int TransactionCount { get; set; }

        /// <summary>
        /// Average transaction value
        /// </summary>
        public double AverageTransaction { get; set; }

        /// <summary>
        /// Number of completed transactions
        /// </summary>
        public int CompletedCount { get; set; }

        /// <summary>
        /// Number of pending transactions
        /// </summary>
        public int PendingCount { get; set; }

        /// <summary>
        /// Number of failed transactions
        /// </summary>
        public int FailedCount { get; set; }
    }

    /// <summary>
    /// Paginated response for transaction list
    /// </summary>
    public class TransactionListResponse
    {
        /// <summary>
        /// List of transactions for current page
        /// </summary>
        public List<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();

        /// <summary>
        /// Total number of transactions matching filters
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Summary statistics for filtered transactions
        /// </summary>
        public TransactionSummary Summary { get; set; } = new TransactionSummary();
    }
}
