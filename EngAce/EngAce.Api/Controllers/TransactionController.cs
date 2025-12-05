using Entities;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Text;

namespace EngAce.Api.Controllers
{
    /// <summary>
    /// Controller for transaction management endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TransactionController> _logger;

        /// <summary>
        /// Constructor for TransactionController
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger instance</param>
        public TransactionController(IConfiguration configuration, ILogger<TransactionController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated list of transactions with filtering and sorting
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 20)</param>
        /// <param name="searchTerm">Search term for user name, email, or transaction ID</param>
        /// <param name="status">Filter by payment status (completed, pending, failed)</param>
        /// <param name="startDate">Filter by start date (inclusive)</param>
        /// <param name="endDate">Filter by end date (inclusive)</param>
        /// <param name="sortBy">Column to sort by (default: created_at)</param>
        /// <param name="sortOrder">Sort order: asc or desc (default: desc)</param>
        /// <returns>Paginated transaction list with summary</returns>
        [HttpGet("list")]
        public async Task<ActionResult<TransactionListResponse>> GetTransactionList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string sortBy = "created_at",
            [FromQuery] string sortOrder = "desc")
        {
            try
            {
                // Validate parameters
                if (page < 1)
                {
                    return BadRequest("Page number must be greater than 0");
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest("Page size must be between 1 and 100");
                }

                // Validate sort order
                sortOrder = sortOrder.ToLower();
                if (sortOrder != "asc" && sortOrder != "desc")
                {
                    return BadRequest("Sort order must be 'asc' or 'desc'");
                }

                // Validate sort column
                var validSortColumns = new[] { "id", "amount", "status", "created_at", "user_name", "user_email" };
                sortBy = sortBy.ToLower();
                if (!validSortColumns.Contains(sortBy))
                {
                    return BadRequest($"Invalid sort column. Valid columns: {string.Join(", ", validSortColumns)}");
                }

                // Validate status if provided
                if (!string.IsNullOrEmpty(status))
                {
                    var validStatuses = new[] { "completed", "pending", "failed" };
                    if (!validStatuses.Contains(status.ToLower()))
                    {
                        return BadRequest($"Invalid status. Valid statuses: {string.Join(", ", validStatuses)}");
                    }
                }

                // Validate date range
                if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
                {
                    return BadRequest("Start date must be before or equal to end date");
                }

                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                var response = new TransactionListResponse
                {
                    Page = page,
                    PageSize = pageSize
                };

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Build WHERE clause
                    var whereConditions = new List<string>();
                    var parameters = new List<MySqlParameter>();

                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        whereConditions.Add("(u.full_name LIKE @searchTerm OR u.email LIKE @searchTerm OR CAST(p.id AS CHAR) LIKE @searchTerm)");
                        parameters.Add(new MySqlParameter("@searchTerm", $"%{searchTerm}%"));
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        whereConditions.Add("p.status = @status");
                        parameters.Add(new MySqlParameter("@status", status.ToLower()));
                    }

                    if (startDate.HasValue)
                    {
                        whereConditions.Add("p.created_at >= @startDate");
                        parameters.Add(new MySqlParameter("@startDate", startDate.Value));
                    }

                    if (endDate.HasValue)
                    {
                        // Include the entire end date (up to 23:59:59)
                        whereConditions.Add("p.created_at < @endDate");
                        parameters.Add(new MySqlParameter("@endDate", endDate.Value.AddDays(1)));
                    }

                    var whereClause = whereConditions.Count > 0 
                        ? "WHERE " + string.Join(" AND ", whereConditions)
                        : "";

                    // Map sort column to actual database column
                    var sortColumn = sortBy switch
                    {
                        "user_name" => "u.full_name",
                        "user_email" => "u.email",
                        _ => $"p.{sortBy}"
                    };

                    // Get total count
                    var countQuery = $@"
                        SELECT COUNT(*) 
                        FROM payments p
                        INNER JOIN users u ON p.user_id = u.id
                        {whereClause}";

                    using (var command = new MySqlCommand(countQuery, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                        var result = await command.ExecuteScalarAsync();
                        response.TotalCount = Convert.ToInt32(result);
                        response.TotalPages = (int)Math.Ceiling(response.TotalCount / (double)pageSize);
                    }

                    // Get summary statistics
                    var summaryQuery = $@"
                        SELECT 
                            COUNT(*) as TotalCount,
                            COALESCE(SUM(CASE WHEN p.status = 'completed' THEN p.amount ELSE 0 END), 0) as TotalRevenue,
                            COUNT(CASE WHEN p.status = 'completed' THEN 1 END) as CompletedCount,
                            COUNT(CASE WHEN p.status = 'pending' THEN 1 END) as PendingCount,
                            COUNT(CASE WHEN p.status = 'failed' THEN 1 END) as FailedCount
                        FROM payments p
                        INNER JOIN users u ON p.user_id = u.id
                        {whereClause}";

                    using (var command = new MySqlCommand(summaryQuery, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var totalCount = reader.GetInt32("TotalCount");
                                var totalRevenue = reader.GetDouble("TotalRevenue");
                                
                                response.Summary = new TransactionSummary
                                {
                                    TotalRevenue = totalRevenue,
                                    TransactionCount = totalCount,
                                    AverageTransaction = totalCount > 0 ? totalRevenue / totalCount : 0,
                                    CompletedCount = reader.GetInt32("CompletedCount"),
                                    PendingCount = reader.GetInt32("PendingCount"),
                                    FailedCount = reader.GetInt32("FailedCount")
                                };
                            }
                        }
                    }

                    // Get paginated transactions
                    var offset = (page - 1) * pageSize;
                    var dataQuery = $@"
                        SELECT 
                            p.id,
                            p.user_id,
                            u.full_name as user_name,
                            u.email as user_email,
                            p.amount,
                            p.status,
                            p.created_at,
                            p.created_at as updated_at
                        FROM payments p
                        INNER JOIN users u ON p.user_id = u.id
                        {whereClause}
                        ORDER BY {sortColumn} {sortOrder.ToUpper()}
                        LIMIT @pageSize OFFSET @offset";

                    using (var command = new MySqlCommand(dataQuery, connection))
                    {
                        command.Parameters.AddRange(parameters.ToArray());
                        command.Parameters.Add(new MySqlParameter("@pageSize", pageSize));
                        command.Parameters.Add(new MySqlParameter("@offset", offset));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                response.Transactions.Add(new TransactionDto
                                {
                                    Id = reader.GetInt32("id").ToString(),
                                    UserId = reader.GetInt32("user_id").ToString(),
                                    UserName = reader.IsDBNull(reader.GetOrdinal("user_name")) 
                                        ? "" 
                                        : reader.GetString("user_name"),
                                    UserEmail = reader.GetString("user_email"),
                                    Amount = Convert.ToDouble(reader.GetDecimal("amount")),
                                    Status = reader.GetString("status"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at")
                                });
                            }
                        }
                    }
                }

                _logger.LogInformation(
                    "Transaction list retrieved: Page {Page}/{TotalPages}, {Count} transactions, Filters: search={Search}, status={Status}, dateRange={StartDate}-{EndDate}",
                    page, response.TotalPages, response.Transactions.Count, searchTerm, status, startDate, endDate);

                return Ok(response);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving transaction list");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving transaction list");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Get detailed information for a single transaction
        /// </summary>
        /// <param name="id">Transaction ID</param>
        /// <returns>Detailed transaction information</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDetailDto>> GetTransactionById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("Transaction ID must be greater than 0");
                }

                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        SELECT 
                            p.id,
                            p.user_id,
                            u.full_name as user_name,
                            u.email as user_email,
                            p.amount,
                            p.status,
                            p.method,
                            p.transaction_history,
                            p.package_id,
                            p.is_lifetime,
                            p.created_at,
                            p.created_at as updated_at
                        FROM payments p
                        INNER JOIN users u ON p.user_id = u.id
                        WHERE p.id = @id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.Add(new MySqlParameter("@id", id));

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var transaction = new TransactionDetailDto
                                {
                                    Id = reader.GetInt32("id").ToString(),
                                    UserId = reader.GetInt32("user_id").ToString(),
                                    UserName = reader.IsDBNull(reader.GetOrdinal("user_name")) 
                                        ? "" 
                                        : reader.GetString("user_name"),
                                    UserEmail = reader.GetString("user_email"),
                                    Amount = Convert.ToDouble(reader.GetDecimal("amount")),
                                    Status = reader.GetString("status"),
                                    PaymentMethod = reader.IsDBNull(reader.GetOrdinal("method")) 
                                        ? null 
                                        : reader.GetString("method"),
                                    TransactionNotes = reader.IsDBNull(reader.GetOrdinal("transaction_history")) 
                                        ? null 
                                        : reader.GetString("transaction_history"),
                                    PackageId = reader.GetInt32("package_id").ToString(),
                                    IsLifetime = reader.GetBoolean("is_lifetime"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at")
                                };

                                _logger.LogInformation("Transaction {Id} retrieved successfully", id);
                                return Ok(transaction);
                            }
                            else
                            {
                                _logger.LogWarning("Transaction {Id} not found", id);
                                return NotFound($"Transaction with ID {id} not found");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving transaction {Id}", id);
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving transaction {Id}", id);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
