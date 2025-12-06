using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System;
using System.Threading.Tasks;
using EngAce.Api.Services;

namespace EngAce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;

        private readonly IPremiumExpirationService _premiumExpirationService;

        public PaymentController(
            IConfiguration configuration, 
            ILogger<PaymentController> logger,
            IPremiumExpirationService premiumExpirationService)
        {
            _configuration = configuration;
            _logger = logger;
            _premiumExpirationService = premiumExpirationService;
        }

        /// <summary>
        /// Check if email exists in the system
        /// </summary>
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    return StatusCode(500, new { message = "Database connection not configured" });
                }

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    using (var command = new MySqlCommand(
                        "SELECT id, email, full_name, account_type, status FROM users WHERE email = @email LIMIT 1", 
                        connection))
                    {
                        command.Parameters.AddWithValue("@email", email);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return Ok(new
                                {
                                    exists = true,
                                    userId = reader.GetInt32("id"),
                                    email = reader.GetString("email"),
                                    fullName = reader.IsDBNull(reader.GetOrdinal("full_name")) 
                                        ? null 
                                        : reader.GetString("full_name"),
                                    accountType = reader.GetString("account_type"),
                                    status = reader.GetString("status")
                                });
                            }
                            else
                            {
                                return Ok(new { exists = false });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email {Email}", email);
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Manually add payment and upgrade user to premium
        /// </summary>
        [HttpPost("manual-payment")]
        public async Task<IActionResult> AddManualPayment([FromBody] ManualPaymentRequest request)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, new { message = "Database connection not configured" });
                }

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {
                            // 1. Find user by email
                            int userId;
                            string currentAccountType;
                            using (var command = new MySqlCommand(
                                "SELECT id, account_type FROM users WHERE email = @email LIMIT 1", 
                                connection, transaction))
                            {
                                command.Parameters.AddWithValue("@email", request.Email);
                                using (var reader = await command.ExecuteReaderAsync())
                                {
                                    if (!await reader.ReadAsync())
                                    {
                                        await transaction.RollbackAsync();
                                        return NotFound(new { message = $"Không tìm thấy user với email: {request.Email}" });
                                    }
                                    userId = reader.GetInt32("id");
                                    currentAccountType = reader.GetString("account_type");
                                }
                            }

                            // 2. Get or create default package
                            int packageId = 1; // Default package ID
                            using (var command = new MySqlCommand(
                                @"SELECT id FROM packages WHERE is_active = 1 LIMIT 1", 
                                connection, transaction))
                            {
                                var result = await command.ExecuteScalarAsync();
                                if (result != null)
                                {
                                    packageId = Convert.ToInt32(result);
                                }
                                else
                                {
                                    // Create default package if not exists
                                    using (var insertCmd = new MySqlCommand(
                                        @"INSERT INTO packages (name, description, price, duration_months, is_active) 
                                          VALUES ('Premium Package', 'Premium manual payment', @amount, NULL, 1)", 
                                        connection, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@amount", request.Amount);
                                        await insertCmd.ExecuteNonQueryAsync();
                                        packageId = (int)insertCmd.LastInsertedId;
                                    }
                                }
                            }

                            // 3. Insert payment record
                            using (var command = new MySqlCommand(
                                @"INSERT INTO payments (user_id, package_id, amount, method, status, is_lifetime, transaction_history, created_at)
                                  VALUES (@userId, @packageId, @amount, @method, 'completed', @isLifetime, @note, NOW())",
                                connection, transaction))
                            {
                                command.Parameters.AddWithValue("@userId", userId);
                                command.Parameters.AddWithValue("@packageId", packageId);
                                command.Parameters.AddWithValue("@amount", request.Amount);
                                command.Parameters.AddWithValue("@method", request.Method ?? "Manual");
                                command.Parameters.AddWithValue("@isLifetime", request.IsLifetime ? 1 : 0);
                                command.Parameters.AddWithValue("@note", request.Note ?? "Manual payment by admin");
                                await command.ExecuteNonQueryAsync();
                            }

                            // 4. Update user to premium
                            DateTime? expiresAt = null;
                            if (!request.IsLifetime && request.DurationMonths > 0)
                            {
                                expiresAt = DateTime.Now.AddMonths(request.DurationMonths);
                            }

                            using (var command = new MySqlCommand(
                                @"UPDATE users 
                                  SET account_type = 'premium', 
                                      premium_expires_at = @expiresAt,
                                      updated_at = NOW()
                                  WHERE id = @userId",
                                connection, transaction))
                            {
                                command.Parameters.AddWithValue("@userId", userId);
                                command.Parameters.AddWithValue("@expiresAt", 
                                    expiresAt.HasValue ? (object)expiresAt.Value : DBNull.Value);
                                await command.ExecuteNonQueryAsync();
                            }

                            await transaction.CommitAsync();

                            _logger.LogInformation(
                                "Manual payment added successfully for user {Email} (ID: {UserId}), Amount: {Amount}, IsLifetime: {IsLifetime}", 
                                request.Email, userId, request.Amount, request.IsLifetime);

                            return Ok(new
                            {
                                success = true,
                                message = $"Đã thêm thanh toán và nâng cấp tài khoản thành công!",
                                userId = userId,
                                email = request.Email,
                                previousAccountType = currentAccountType,
                                newAccountType = "premium",
                                amount = request.Amount,
                                isLifetime = request.IsLifetime,
                                expiresAt = expiresAt
                            });
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while adding manual payment for {Email}", request.Email);
                return StatusCode(500, new { message = $"Database error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding manual payment for {Email}", request.Email);
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get all payments with user info
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllPayments(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 20,
            [FromQuery] string? status = null)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                if (string.IsNullOrEmpty(connectionString))
                {
                    return StatusCode(500, new { message = "Database connection not configured" });
                }

                var payments = new List<object>();
                int totalCount = 0;

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Get total count
                    var countQuery = status != null 
                        ? "SELECT COUNT(*) FROM payments WHERE status = @status"
                        : "SELECT COUNT(*) FROM payments";
                    
                    using (var command = new MySqlCommand(countQuery, connection))
                    {
                        if (status != null)
                        {
                            command.Parameters.AddWithValue("@status", status);
                        }
                        totalCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }

                    // Get payments with user info
                    var query = @"
                        SELECT 
                            p.id,
                            p.user_id,
                            p.amount,
                            p.method,
                            p.status,
                            p.is_lifetime,
                            p.created_at,
                            u.email,
                            u.full_name,
                            u.account_type
                        FROM payments p
                        INNER JOIN users u ON p.user_id = u.id
                        " + (status != null ? "WHERE p.status = @status " : "") + @"
                        ORDER BY p.created_at DESC
                        LIMIT @offset, @pageSize";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        if (status != null)
                        {
                            command.Parameters.AddWithValue("@status", status);
                        }
                        command.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
                        command.Parameters.AddWithValue("@pageSize", pageSize);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                payments.Add(new
                                {
                                    id = reader.GetInt32("id"),
                                    userId = reader.GetInt32("user_id"),
                                    email = reader.GetString("email"),
                                    fullName = reader.IsDBNull(reader.GetOrdinal("full_name")) 
                                        ? null 
                                        : reader.GetString("full_name"),
                                    amount = reader.GetDecimal("amount"),
                                    method = reader.IsDBNull(reader.GetOrdinal("method")) 
                                        ? null 
                                        : reader.GetString("method"),
                                    status = reader.GetString("status"),
                                    isLifetime = reader.GetBoolean("is_lifetime"),
                                    accountType = reader.GetString("account_type"),
                                    createdAt = reader.GetDateTime("created_at")
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    payments = payments,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting payments");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Manually trigger premium expiration check and downgrade expired users
        /// </summary>
        [HttpPost("check-expired-premium")]
        public async Task<IActionResult> CheckExpiredPremium()
        {
            try
            {
                _logger.LogInformation("Manual premium expiration check triggered");

                var result = await _premiumExpirationService.CheckAndDowngradeExpiredUsersAsync();

                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        checkedAt = result.CheckedAt,
                        totalChecked = result.TotalChecked,
                        totalDowngraded = result.TotalDowngraded,
                        expiredUsers = result.ExpiredUsers
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual premium expiration check");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get list of users whose premium will expire soon (within next 7 days)
        /// </summary>
        [HttpGet("expiring-soon")]
        public async Task<IActionResult> GetExpiringSoonUsers([FromQuery] int days = 7)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                if (string.IsNullOrEmpty(connectionString))
                {
                    return StatusCode(500, new { message = "Database connection not configured" });
                }

                var expiringSoon = new List<object>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        SELECT 
                            id,
                            email,
                            full_name,
                            account_type,
                            premium_expires_at,
                            DATEDIFF(premium_expires_at, NOW()) as days_remaining
                        FROM users
                        WHERE account_type = 'premium'
                        AND premium_expires_at IS NOT NULL
                        AND premium_expires_at > NOW()
                        AND premium_expires_at <= DATE_ADD(NOW(), INTERVAL @days DAY)
                        AND status = 'active'
                        ORDER BY premium_expires_at ASC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@days", days);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                expiringSoon.Add(new
                                {
                                    userId = reader.GetInt32("id"),
                                    email = reader.GetString("email"),
                                    fullName = reader.IsDBNull(reader.GetOrdinal("full_name"))
                                        ? null
                                        : reader.GetString("full_name"),
                                    accountType = reader.GetString("account_type"),
                                    expiresAt = reader.GetDateTime("premium_expires_at"),
                                    daysRemaining = reader.GetInt32("days_remaining")
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    success = true,
                    totalExpiringSoon = expiringSoon.Count,
                    withinDays = days,
                    users = expiringSoon
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring soon users");
                return StatusCode(500, new { message = $"An error occurred: {ex.Message}" });
            }
        }
    }

    public class ManualPaymentRequest
    {
        public string Email { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Method { get; set; }
        public bool IsLifetime { get; set; } = true;
        public int DurationMonths { get; set; } = 0;
        public string? Note { get; set; }
    }
}
