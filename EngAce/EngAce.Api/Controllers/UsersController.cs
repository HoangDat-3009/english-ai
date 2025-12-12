using Entities;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace EngAce.Api.Controllers
{
    /// <summary>
    /// Controller for user management
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsersController> _logger;

        /// <summary>
        /// Constructor for UsersController
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger instance</param>
        public UsersController(IConfiguration configuration, ILogger<UsersController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Get all users from the system with pagination
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <param name="accountType">Filter by account type: free, premium (optional)</param>
        /// <param name="search">Search by name, username, or ID (optional)</param>
        /// <param name="status">Filter by status: active, inactive, banned (optional)</param>
        /// <returns>Paginated list of users</returns>
        [HttpGet]
        public async Task<ActionResult> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? accountType = null,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var users = new List<dynamic>();
                int totalCount = 0;

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Build WHERE clause
                    var whereConditions = new List<string>();
                    if (!string.IsNullOrEmpty(accountType))
                        whereConditions.Add("u.account_type = @AccountType");
                    
                    if (!string.IsNullOrEmpty(search))
                        whereConditions.Add("(u.full_name LIKE @Search OR u.username LIKE @Search OR u.id LIKE @Search)");
                    
                    if (!string.IsNullOrEmpty(status))
                        whereConditions.Add("u.status = @Status");
                    
                    var whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";

                    // Get total count
                    var countQuery = $"SELECT COUNT(*) FROM users u {whereClause}";
                    using (var countCommand = new MySqlCommand(countQuery, connection))
                    {
                        if (!string.IsNullOrEmpty(accountType))
                            countCommand.Parameters.AddWithValue("@AccountType", accountType);
                        if (!string.IsNullOrEmpty(search))
                            countCommand.Parameters.AddWithValue("@Search", $"%{search}%");
                        if (!string.IsNullOrEmpty(status))
                            countCommand.Parameters.AddWithValue("@Status", status);
                        
                        totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                    }

                    // Get paginated data
                    var offset = (page - 1) * pageSize;
                    var query = $@"SELECT u.id, u.username, u.email, u.phone, u.account_type, u.status, 
                                         u.full_name, u.avatar_url, u.total_xp, u.premium_expires_at
                                  FROM users u
                                  {whereClause}
                                  ORDER BY u.id DESC
                                  LIMIT @PageSize OFFSET @Offset";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(accountType))
                            command.Parameters.AddWithValue("@AccountType", accountType);
                        if (!string.IsNullOrEmpty(search))
                            command.Parameters.AddWithValue("@Search", $"%{search}%");
                        if (!string.IsNullOrEmpty(status))
                            command.Parameters.AddWithValue("@Status", status);
                        
                        command.Parameters.AddWithValue("@PageSize", pageSize);
                        command.Parameters.AddWithValue("@Offset", offset);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var user = new
                                {
                                    UserID = reader.GetInt32("id"),
                                    Username = reader.GetString("username"),
                                    Email = reader.GetString("email"),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("phone")) 
                                        ? null 
                                        : reader.GetString("phone"),
                                    AccountType = reader.GetString("account_type"),
                                    Status = reader.GetString("status"),
                                    FullName = reader.IsDBNull(reader.GetOrdinal("full_name"))
                                        ? null
                                        : reader.GetString("full_name"),
                                    Avatar = reader.IsDBNull(reader.GetOrdinal("avatar_url"))
                                        ? null
                                        : reader.GetString("avatar_url"),
                                    TotalXP = reader.GetInt32("total_xp"),
                                    PremiumExpiresAt = reader.IsDBNull(reader.GetOrdinal("premium_expires_at"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime("premium_expires_at")
                                };
                                users.Add(user);
                            }
                        }
                    }
                }

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                _logger.LogInformation("Retrieved {Count} users (page {Page}/{TotalPages})", users.Count, page, totalPages);
                
                return Ok(new
                {
                    Data = users,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasPrevious = page > 1,
                        HasNext = page < totalPages
                    }
                });
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving users");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving users");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"SELECT id, username, email, phone, account_type, status, 
                                         full_name, bio, address, avatar_url, total_study_time, 
                                         total_xp, premium_expires_at, last_active_at, created_at, updated_at
                                  FROM users 
                                  WHERE id = @UserId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var user = new User
                                {
                                    UserID = reader.GetInt32("id"),
                                    Username = reader.GetString("username"),
                                    Email = reader.GetString("email"),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("phone")) 
                                        ? null 
                                        : reader.GetString("phone"),
                                    AccountType = reader.GetString("account_type"),
                                    Status = reader.GetString("status"),
                                    FullName = reader.IsDBNull(reader.GetOrdinal("full_name"))
                                        ? null
                                        : reader.GetString("full_name"),
                                    Bio = reader.IsDBNull(reader.GetOrdinal("bio"))
                                        ? null
                                        : reader.GetString("bio"),
                                    Address = reader.IsDBNull(reader.GetOrdinal("address"))
                                        ? null
                                        : reader.GetString("address"),
                                    Avatar = reader.IsDBNull(reader.GetOrdinal("avatar_url"))
                                        ? null
                                        : reader.GetString("avatar_url"),
                                    TotalStudyTime = reader.GetInt32("total_study_time"),
                                    TotalXP = reader.GetInt32("total_xp"),
                                    PremiumExpiresAt = reader.IsDBNull(reader.GetOrdinal("premium_expires_at"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime("premium_expires_at"),
                                    LastActiveAt = reader.IsDBNull(reader.GetOrdinal("last_active_at"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime("last_active_at"),
                                    CreatedAt = reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.GetDateTime("updated_at")
                                };

                                _logger.LogInformation("Retrieved user {UserId}", id);
                                return Ok(user);
                            }
                            else
                            {
                                _logger.LogWarning("User {UserId} not found", id);
                                return NotFound($"User with ID {id} not found");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving user {UserId}", id);
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user {UserId}", id);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Get user profile with full details
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User with profile details</returns>
        [HttpGet("{id}/profile")]
        public async Task<ActionResult<object>> GetUserProfile(int id)
        {
            try
            {
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
                            id, username, email, phone, account_type, status,
                            full_name, avatar_url, bio, address,
                            total_study_time, total_xp, premium_expires_at, 
                            last_active_at, created_at, updated_at
                        FROM users
                        WHERE id = @UserId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var userProfile = new
                                {
                                    UserID = reader.GetInt32("id"),
                                    Username = reader.GetString("username"),
                                    Email = reader.GetString("email"),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("phone")) 
                                        ? null 
                                        : reader.GetString("phone"),
                                    AccountType = reader.GetString("account_type"),
                                    Status = reader.GetString("status"),
                                    
                                    // Profile fields
                                    FullName = reader.IsDBNull(reader.GetOrdinal("full_name")) 
                                        ? null 
                                        : reader.GetString("full_name"),
                                    Avatar = reader.IsDBNull(reader.GetOrdinal("avatar_url")) 
                                        ? null 
                                        : reader.GetString("avatar_url"),
                                    Address = reader.IsDBNull(reader.GetOrdinal("address")) 
                                        ? null 
                                        : reader.GetString("address"),
                                    Bio = reader.IsDBNull(reader.GetOrdinal("bio")) 
                                        ? null 
                                        : reader.GetString("bio"),
                                    TotalStudyTime = reader.GetInt32("total_study_time"),
                                    TotalXP = reader.GetInt32("total_xp"),
                                    PremiumExpiresAt = reader.IsDBNull(reader.GetOrdinal("premium_expires_at"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime("premium_expires_at"),
                                    LastActiveAt = reader.IsDBNull(reader.GetOrdinal("last_active_at"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime("last_active_at"),
                                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("created_at")) 
                                        ? (DateTime?)null 
                                        : reader.GetDateTime("created_at"),
                                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) 
                                        ? (DateTime?)null 
                                        : reader.GetDateTime("updated_at")
                                };

                                _logger.LogInformation("Retrieved user profile {UserId}", id);
                                return Ok(userProfile);
                            }
                            else
                            {
                                _logger.LogWarning("User {UserId} not found", id);
                                return NotFound($"User with ID {id} not found");
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving user profile {UserId}", id);
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user profile {UserId}", id);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Get users by account type
        /// </summary>
        /// <param name="accountType">Account type (free, premium)</param>
        /// <returns>List of users with the specified account type</returns>
        [HttpGet("account-type/{accountType}")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByAccountType(string accountType)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                var users = new List<dynamic>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"SELECT u.id, u.username, u.email, u.phone, u.account_type, u.status,
                                         u.full_name, u.avatar_url, u.total_xp
                                  FROM users u
                                  WHERE u.account_type = @AccountType
                                  ORDER BY u.id DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AccountType", accountType);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var user = new
                                {
                                    UserID = reader.GetInt32("id"),
                                    Username = reader.GetString("username"),
                                    Email = reader.GetString("email"),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("phone")) 
                                        ? null 
                                        : reader.GetString("phone"),
                                    AccountType = reader.GetString("account_type"),
                                    Status = reader.GetString("status"),
                                    FullName = reader.IsDBNull(reader.GetOrdinal("full_name"))
                                        ? null
                                        : reader.GetString("full_name"),
                                    Avatar = reader.IsDBNull(reader.GetOrdinal("avatar_url"))
                                        ? null
                                        : reader.GetString("avatar_url"),
                                    TotalXP = reader.GetInt32("total_xp")
                                };
                                users.Add(user);
                            }
                        }
                    }
                }

                _logger.LogInformation("Retrieved {Count} users with account type {AccountType}", users.Count, accountType);
                return Ok(users);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving users by account type {AccountType}", accountType);
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving users by account type {AccountType}", accountType);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Update user status (active, inactive, banned) with reason tracking
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="request">Request containing new status, reason code, and note</param>
        /// <returns>Success message</returns>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                // Validate status value
                var validStatuses = new[] { "active", "inactive", "banned" };
                if (!validStatuses.Contains(request.Status.ToLower()))
                {
                    return BadRequest($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");
                }

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {
                            // Get current status
                            string? currentStatus = null;
                            var getStatusQuery = "SELECT status FROM users WHERE id = @UserId";
                            using (var getStatusCommand = new MySqlCommand(getStatusQuery, connection, transaction))
                            {
                                getStatusCommand.Parameters.AddWithValue("@UserId", id);
                                var result = await getStatusCommand.ExecuteScalarAsync();
                                
                                if (result == null)
                                {
                                    await transaction.RollbackAsync();
                                    _logger.LogWarning("Attempted to update status for non-existent user {UserId}", id);
                                    return NotFound($"User with ID {id} not found");
                                }
                                
                                currentStatus = result.ToString();
                            }

                            // Skip if status hasn't changed
                            if (currentStatus?.ToLower() == request.Status.ToLower())
                            {
                                await transaction.CommitAsync();
                                return Ok(new { 
                                    message = "Status unchanged",
                                    userId = id,
                                    currentStatus = currentStatus
                                });
                            }

                            // Update status
                            var updateQuery = "UPDATE users SET status = @Status WHERE id = @UserId";
                            using (var updateCommand = new MySqlCommand(updateQuery, connection, transaction))
                            {
                                updateCommand.Parameters.AddWithValue("@Status", request.Status.ToLower());
                                updateCommand.Parameters.AddWithValue("@UserId", id);
                                await updateCommand.ExecuteNonQueryAsync();
                            }

                            // Insert into user_status_history
                            var historyQuery = @"INSERT INTO user_status_history 
                                (user_id, from_status, to_status, reason_code, reason_note, changed_by, changed_at) 
                                VALUES (@UserId, @FromStatus, @ToStatus, @ReasonCode, @ReasonNote, @ChangedBy, NOW())";
                            
                            using (var historyCommand = new MySqlCommand(historyQuery, connection, transaction))
                            {
                                historyCommand.Parameters.AddWithValue("@UserId", id);
                                historyCommand.Parameters.AddWithValue("@FromStatus", currentStatus);
                                historyCommand.Parameters.AddWithValue("@ToStatus", request.Status.ToLower());
                                historyCommand.Parameters.AddWithValue("@ReasonCode", 
                                    string.IsNullOrEmpty(request.ReasonCode) ? DBNull.Value : request.ReasonCode);
                                historyCommand.Parameters.AddWithValue("@ReasonNote", 
                                    string.IsNullOrEmpty(request.ReasonNote) ? DBNull.Value : request.ReasonNote);
                                historyCommand.Parameters.AddWithValue("@ChangedBy", 
                                    request.ChangedByUserID.HasValue ? request.ChangedByUserID.Value : DBNull.Value);
                                
                                await historyCommand.ExecuteNonQueryAsync();
                            }

                            await transaction.CommitAsync();
                            
                            _logger.LogInformation(
                                "Successfully updated user {UserId} status from {FromStatus} to {ToStatus} with reason {ReasonCode}", 
                                id, currentStatus, request.Status, request.ReasonCode ?? "None");
                            
                            return Ok(new { 
                                message = $"User status updated from '{currentStatus}' to '{request.Status}' successfully",
                                userId = id,
                                previousStatus = currentStatus,
                                newStatus = request.Status.ToLower(),
                                reason = request.ReasonNote
                            });
                        }
                        catch
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while updating user {UserId} status", id);
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user {UserId} status", id);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Get status change history for a specific user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>List of status changes</returns>
        [HttpGet("{id}/status-history")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserStatusHistory(int id)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                var history = new List<object>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"SELECT 
                                    h.id, 
                                    h.from_status, 
                                    h.to_status, 
                                    h.reason_code,
                                    h.reason_note, 
                                    h.changed_by,
                                    u.username as changed_by_username,
                                    h.changed_at
                                  FROM user_status_history h
                                  LEFT JOIN users u ON h.changed_by = u.id
                                  WHERE h.user_id = @UserId
                                  ORDER BY h.changed_at DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var record = new
                                {
                                    HistoryID = reader.GetInt32("id"),
                                    FromStatus = reader.IsDBNull(reader.GetOrdinal("from_status")) 
                                        ? null 
                                        : reader.GetString("from_status"),
                                    ToStatus = reader.GetString("to_status"),
                                    ReasonCode = reader.IsDBNull(reader.GetOrdinal("reason_code")) 
                                        ? null 
                                        : reader.GetString("reason_code"),
                                    ReasonNote = reader.IsDBNull(reader.GetOrdinal("reason_note")) 
                                        ? null 
                                        : reader.GetString("reason_note"),
                                    ChangedBy = reader.IsDBNull(reader.GetOrdinal("changed_by")) 
                                        ? (int?)null 
                                        : reader.GetInt32("changed_by"),
                                    ChangedByUsername = reader.IsDBNull(reader.GetOrdinal("changed_by_username")) 
                                        ? null 
                                        : reader.GetString("changed_by_username"),
                                    ChangedAt = reader.GetDateTime("changed_at")
                                };
                                history.Add(record);
                            }
                        }
                    }
                }

                _logger.LogInformation("Retrieved {Count} status history records for user {UserId}", history.Count, id);
                return Ok(history);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving status history for user {UserId}", id);
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving status history for user {UserId}", id);
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    /// <summary>
    /// Get user statistics for admin dashboard
    /// </summary>
    /// <returns>Statistics object</returns>
    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetUserStatistics()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("LearningSystemDb");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Database connection string is not configured");
                return StatusCode(500, "Database connection not configured");
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Get all statistics in one query for better performance
                var query = @"
                    SELECT 
                        COUNT(*) as TotalUsers,
                        COUNT(CASE WHEN status = 'active' THEN 1 END) as ActiveUsers,
                        COUNT(CASE WHEN created_at >= DATE_FORMAT(NOW(), '%Y-%m-01') THEN 1 END) as NewThisMonth,
                        COUNT(CASE WHEN account_type = 'premium' THEN 1 END) as PremiumUsers,
                        COUNT(CASE WHEN status = 'inactive' THEN 1 END) as InactiveUsers
                    FROM users";

                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var stats = new
                            {
                                TotalUsers = reader.GetInt32("TotalUsers"),
                                ActiveUsers = reader.GetInt32("ActiveUsers"),
                                NewThisMonth = reader.GetInt32("NewThisMonth"),
                                PremiumUsers = reader.GetInt32("PremiumUsers"),
                                InactiveUsers = reader.GetInt32("InactiveUsers")
                            };

                            _logger.LogInformation("Retrieved user statistics");
                            return Ok(stats);
                        }
                        else
                        {
                            return Ok(new
                            {
                                TotalUsers = 0,
                                ActiveUsers = 0,
                                NewThisMonth = 0,
                                PremiumUsers = 0,
                                InactiveUsers = 0
                            });
                        }
                    }
                }
            }
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "MySQL error occurred while retrieving user statistics");
            return StatusCode(500, $"Database error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user statistics");
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Get detailed chart data for user analytics
    /// </summary>
    /// <returns>Chart data including monthly growth, status distribution, account type distribution, and XP distribution</returns>
    [HttpGet("charts")]
    public async Task<ActionResult<object>> GetUserCharts()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("LearningSystemDb");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Database connection string is not configured");
                return StatusCode(500, "Database connection not configured");
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // 1. Status Distribution
                var statusDistribution = new List<object>();
                var statusQuery = @"
                    SELECT 
                        status,
                        COUNT(*) as count
                    FROM users
                    GROUP BY status";
                
                using (var command = new MySqlCommand(statusQuery, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            statusDistribution.Add(new
                            {
                                status = reader.GetString("status"),
                                count = reader.GetInt32("count")
                            });
                        }
                    }
                }

                // 2. Account Type Distribution
                var accountTypeDistribution = new List<object>();
                var accountTypeQuery = @"
                    SELECT 
                        account_type,
                        COUNT(*) as count
                    FROM users
                    GROUP BY account_type";
                
                using (var command = new MySqlCommand(accountTypeQuery, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            accountTypeDistribution.Add(new
                            {
                                accountType = reader.GetString("account_type"),
                                count = reader.GetInt32("count")
                            });
                        }
                    }
                }

                // 3. Monthly Growth (last 6 months)
                var monthlyGrowth = new List<object>();
                var monthlyQuery = @"
                    SELECT 
                        DATE_FORMAT(created_at, '%Y-%m') as month,
                        COUNT(*) as count
                    FROM users
                    WHERE created_at >= DATE_SUB(NOW(), INTERVAL 6 MONTH)
                    GROUP BY DATE_FORMAT(created_at, '%Y-%m')
                    ORDER BY month ASC";
                
                using (var command = new MySqlCommand(monthlyQuery, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            monthlyGrowth.Add(new
                            {
                                month = reader.GetString("month"),
                                count = reader.GetInt32("count")
                            });
                        }
                    }
                }

                // 4. XP Distribution
                var xpDistribution = new List<object>();
                var xpQuery = @"
                    SELECT 
                        CASE 
                            WHEN total_xp BETWEEN 0 AND 100 THEN '0-100'
                            WHEN total_xp BETWEEN 101 AND 500 THEN '101-500'
                            WHEN total_xp BETWEEN 501 AND 1000 THEN '501-1000'
                            ELSE '1000+'
                        END as xp_range,
                        COUNT(*) as count
                    FROM users
                    GROUP BY xp_range
                    ORDER BY 
                        CASE xp_range
                            WHEN '0-100' THEN 1
                            WHEN '101-500' THEN 2
                            WHEN '501-1000' THEN 3
                            WHEN '1000+' THEN 4
                        END";
                
                using (var command = new MySqlCommand(xpQuery, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            xpDistribution.Add(new
                            {
                                range = reader.GetString("xp_range"),
                                count = reader.GetInt32("count")
                            });
                        }
                    }
                }

                var chartData = new
                {
                    StatusDistribution = statusDistribution,
                    AccountTypeDistribution = accountTypeDistribution,
                    MonthlyGrowth = monthlyGrowth,
                    XpDistribution = xpDistribution
                };

                _logger.LogInformation("Retrieved user chart data");
                return Ok(chartData);
            }
        }
        catch (MySqlException ex)
        {
            _logger.LogError(ex, "MySQL error occurred while retrieving user chart data");
            return StatusCode(500, $"Database error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user chart data");
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}

/// <summary>
/// Request model for updating user status
/// </summary>
public class UpdateStatusRequest
{
    /// <summary>
    /// New status value (active, inactive, banned)
    /// </summary>
    public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// Reason code from UserStatusReason table (e.g., USER_REQUEST, SECURITY, etc.)
        /// </summary>
        public string? ReasonCode { get; set; }
        
        /// <summary>
        /// Additional note/reason entered by admin
        /// </summary>
        public string? ReasonNote { get; set; }
        
        /// <summary>
        /// ID of the admin/user who made the change
        /// </summary>
        public int? ChangedByUserID { get; set; }
    }
}
