using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EngAce.Api.Services.Premium
{
    /// <summary>
    /// Service to handle premium expiration checking and downgrade
    /// </summary>
    public interface IPremiumExpirationService
    {
        Task<PremiumExpirationResult> CheckAndDowngradeExpiredUsersAsync();
    }

    public class PremiumExpirationService : IPremiumExpirationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PremiumExpirationService> _logger;

        public PremiumExpirationService(IConfiguration configuration, ILogger<PremiumExpirationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Check and downgrade all expired premium users
        /// </summary>
        public async Task<PremiumExpirationResult> CheckAndDowngradeExpiredUsersAsync()
        {
            var result = new PremiumExpirationResult
            {
                CheckedAt = DateTime.UtcNow,
                ExpiredUsers = new List<ExpiredUserInfo>()
            };

            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    result.Success = false;
                    result.Message = "Database connection not configured";
                    return result;
                }

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Find all premium users with expired premium_expires_at
                    var query = @"
                        SELECT 
                            id, 
                            email, 
                            full_name,
                            account_type,
                            premium_expires_at
                        FROM users
                        WHERE account_type = 'premium'
                        AND premium_expires_at IS NOT NULL
                        AND premium_expires_at < NOW()
                        AND status = 'active'";

                    var expiredUserIds = new List<int>();

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var userId = reader.GetInt32("id");
                                expiredUserIds.Add(userId);

                                result.ExpiredUsers.Add(new ExpiredUserInfo
                                {
                                    UserId = userId,
                                    Email = reader.GetString("email"),
                                    FullName = reader.IsDBNull(reader.GetOrdinal("full_name"))
                                        ? null
                                        : reader.GetString("full_name"),
                                    ExpiredAt = reader.GetDateTime("premium_expires_at")
                                });
                            }
                        }
                    }

                    if (expiredUserIds.Count == 0)
                    {
                        _logger.LogInformation("No expired premium users found");
                        result.Success = true;
                        result.Message = "No expired users found";
                        result.TotalChecked = 0;
                        result.TotalDowngraded = 0;
                        return result;
                    }

                    // Downgrade expired users to free
                    var updateQuery = @"
                        UPDATE users
                        SET account_type = 'free',
                            premium_expires_at = NULL,
                            updated_at = NOW()
                        WHERE id IN (" + string.Join(",", expiredUserIds) + ")";

                    int updatedCount;
                    using (var updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        updatedCount = await updateCommand.ExecuteNonQueryAsync();
                    }

                    result.Success = true;
                    result.TotalChecked = expiredUserIds.Count;
                    result.TotalDowngraded = updatedCount;
                    result.Message = $"Successfully downgraded {updatedCount} expired premium users to free";

                    _logger.LogInformation(
                        "Premium expiration check completed: {TotalChecked} expired users found, {TotalDowngraded} downgraded",
                        result.TotalChecked, result.TotalDowngraded);

                    // Log each downgraded user
                    foreach (var user in result.ExpiredUsers)
                    {
                        _logger.LogInformation(
                            "User {UserId} ({Email}) downgraded from premium to free. Expired at: {ExpiredAt}",
                            user.UserId, user.Email, user.ExpiredAt);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking and downgrading expired premium users");
                result.Success = false;
                result.Message = $"Error: {ex.Message}";
                return result;
            }
        }
    }

    public class PremiumExpirationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CheckedAt { get; set; }
        public int TotalChecked { get; set; }
        public int TotalDowngraded { get; set; }
        public List<ExpiredUserInfo> ExpiredUsers { get; set; } = new();
    }

    public class ExpiredUserInfo
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public DateTime ExpiredAt { get; set; }
    }
}
