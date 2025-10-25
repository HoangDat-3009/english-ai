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
        /// Get all users from the system
        /// </summary>
        /// <returns>List of users</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
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

                    var query = @"SELECT u.UserID, u.Username, u.Email, u.Phone, u.Role, u.Status, 
                                         up.FullName
                                  FROM User u
                                  LEFT JOIN UserProfile up ON u.UserID = up.UserID
                                  ORDER BY u.UserID DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var user = new
                                {
                                    UserID = reader.GetInt32("UserID"),
                                    Username = reader.GetString("Username"),
                                    Email = reader.GetString("Email"),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) 
                                        ? null 
                                        : reader.GetString("Phone"),
                                    Role = reader.GetString("Role"),
                                    Status = reader.GetString("Status"),
                                    FullName = reader.IsDBNull(reader.GetOrdinal("FullName"))
                                        ? null
                                        : reader.GetString("FullName")
                                };
                                users.Add(user);
                            }
                        }
                    }
                }

                _logger.LogInformation("Retrieved {Count} users", users.Count);
                return Ok(users);
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

                    var query = @"SELECT UserID, Username, Email, Phone, Role, Status 
                                  FROM User 
                                  WHERE UserID = @UserId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var user = new User
                                {
                                    UserID = reader.GetInt32("UserID"),
                                    Username = reader.GetString("Username"),
                                    Email = reader.GetString("Email"),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) 
                                        ? null 
                                        : reader.GetString("Phone"),
                                    Role = reader.GetString("Role"),
                                    Status = reader.GetString("Status")
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
                            u.UserID, u.Username, u.Email, u.Phone, u.Role, u.Status,
                            p.FullName, p.AvatarURL, p.DOB, p.Gender, p.Address, 
                            p.Bio, p.PreferredLevel, p.LearningGoal, p.Timezone, 
                            p.Locale, p.CreatedAt, p.UpdatedAt,
                            pref.EmailNotify, pref.DarkMode
                        FROM User u
                        LEFT JOIN UserProfile p ON u.UserID = p.UserID
                        LEFT JOIN UserPreference pref ON u.UserID = pref.UserID
                        WHERE u.UserID = @UserId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var userProfile = new
                                {
                                    UserID = reader.GetInt32("UserID"),
                                    Username = reader.GetString("Username"),
                                    Email = reader.GetString("Email"),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) 
                                        ? null 
                                        : reader.GetString("Phone"),
                                    Role = reader.GetString("Role"),
                                    Status = reader.GetString("Status"),
                                    
                                    // Profile fields
                                    FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) 
                                        ? null 
                                        : reader.GetString("FullName"),
                                    AvatarURL = reader.IsDBNull(reader.GetOrdinal("AvatarURL")) 
                                        ? null 
                                        : reader.GetString("AvatarURL"),
                                    DOB = reader.IsDBNull(reader.GetOrdinal("DOB")) 
                                        ? (DateTime?)null 
                                        : reader.GetDateTime("DOB"),
                                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) 
                                        ? null 
                                        : reader.GetString("Gender"),
                                    Address = reader.IsDBNull(reader.GetOrdinal("Address")) 
                                        ? null 
                                        : reader.GetString("Address"),
                                    Bio = reader.IsDBNull(reader.GetOrdinal("Bio")) 
                                        ? null 
                                        : reader.GetString("Bio"),
                                    PreferredLevel = reader.IsDBNull(reader.GetOrdinal("PreferredLevel")) 
                                        ? null 
                                        : reader.GetString("PreferredLevel"),
                                    LearningGoal = reader.IsDBNull(reader.GetOrdinal("LearningGoal")) 
                                        ? null 
                                        : reader.GetString("LearningGoal"),
                                    Timezone = reader.IsDBNull(reader.GetOrdinal("Timezone")) 
                                        ? "Asia/Ho_Chi_Minh" 
                                        : reader.GetString("Timezone"),
                                    Locale = reader.IsDBNull(reader.GetOrdinal("Locale")) 
                                        ? "vi-VN" 
                                        : reader.GetString("Locale"),
                                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) 
                                        ? (DateTime?)null 
                                        : reader.GetDateTime("CreatedAt"),
                                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) 
                                        ? (DateTime?)null 
                                        : reader.GetDateTime("UpdatedAt"),
                                    
                                    // Preference fields
                                    EmailNotify = reader.IsDBNull(reader.GetOrdinal("EmailNotify")) 
                                        ? true 
                                        : reader.GetBoolean("EmailNotify"),
                                    DarkMode = reader.IsDBNull(reader.GetOrdinal("DarkMode")) 
                                        ? false 
                                        : reader.GetBoolean("DarkMode")
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
        /// Get users by role
        /// </summary>
        /// <param name="role">User role (admin, student, teacher)</param>
        /// <returns>List of users with the specified role</returns>
        [HttpGet("role/{role}")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByRole(string role)
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

                    var query = @"SELECT u.UserID, u.Username, u.Email, u.Phone, u.Role, u.Status,
                                         up.FullName
                                  FROM User u
                                  LEFT JOIN UserProfile up ON u.UserID = up.UserID
                                  WHERE u.Role = @Role
                                  ORDER BY u.UserID DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Role", role);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var user = new
                                {
                                    UserID = reader.GetInt32("UserID"),
                                    Username = reader.GetString("Username"),
                                    Email = reader.GetString("Email"),
                                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) 
                                        ? null 
                                        : reader.GetString("Phone"),
                                    Role = reader.GetString("Role"),
                                    Status = reader.GetString("Status"),
                                    FullName = reader.IsDBNull(reader.GetOrdinal("FullName"))
                                        ? null
                                        : reader.GetString("FullName")
                                };
                                users.Add(user);
                            }
                        }
                    }
                }

                _logger.LogInformation("Retrieved {Count} users with role {Role}", users.Count, role);
                return Ok(users);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving users by role {Role}", role);
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving users by role {Role}", role);
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
                            var getStatusQuery = "SELECT Status FROM User WHERE UserID = @UserId";
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
                            var updateQuery = "UPDATE User SET Status = @Status WHERE UserID = @UserId";
                            using (var updateCommand = new MySqlCommand(updateQuery, connection, transaction))
                            {
                                updateCommand.Parameters.AddWithValue("@Status", request.Status.ToLower());
                                updateCommand.Parameters.AddWithValue("@UserId", id);
                                await updateCommand.ExecuteNonQueryAsync();
                            }

                            // Insert into UserStatusHistory
                            var historyQuery = @"INSERT INTO UserStatusHistory 
                                (UserID, FromStatus, ToStatus, ReasonCode, ReasonNote, ChangedByUserID, ChangedAt) 
                                VALUES (@UserId, @FromStatus, @ToStatus, @ReasonCode, @ReasonNote, @ChangedByUserID, NOW())";
                            
                            using (var historyCommand = new MySqlCommand(historyQuery, connection, transaction))
                            {
                                historyCommand.Parameters.AddWithValue("@UserId", id);
                                historyCommand.Parameters.AddWithValue("@FromStatus", currentStatus);
                                historyCommand.Parameters.AddWithValue("@ToStatus", request.Status.ToLower());
                                historyCommand.Parameters.AddWithValue("@ReasonCode", 
                                    string.IsNullOrEmpty(request.ReasonCode) ? DBNull.Value : request.ReasonCode);
                                historyCommand.Parameters.AddWithValue("@ReasonNote", 
                                    string.IsNullOrEmpty(request.ReasonNote) ? DBNull.Value : request.ReasonNote);
                                historyCommand.Parameters.AddWithValue("@ChangedByUserID", 
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
        /// Get all available status reason codes
        /// </summary>
        /// <returns>List of status reasons</returns>
        [HttpGet("status-reasons")]
        public async Task<ActionResult<IEnumerable<object>>> GetStatusReasons()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                var reasons = new List<object>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"SELECT ReasonCode, ReasonName, Description, IsTemporary 
                                  FROM UserStatusReason 
                                  ORDER BY ReasonCode";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var reason = new
                                {
                                    ReasonCode = reader.GetString("ReasonCode"),
                                    ReasonName = reader.GetString("ReasonName"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                                        ? null 
                                        : reader.GetString("Description"),
                                    IsTemporary = reader.GetBoolean("IsTemporary")
                                };
                                reasons.Add(reason);
                            }
                        }
                    }
                }

                _logger.LogInformation("Retrieved {Count} status reasons", reasons.Count);
                return Ok(reasons);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving status reasons");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving status reasons");
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
                                    h.HistoryID, 
                                    h.FromStatus, 
                                    h.ToStatus, 
                                    h.ReasonCode,
                                    r.ReasonName,
                                    h.ReasonNote, 
                                    h.ExpiresAt,
                                    h.ChangedByUserID,
                                    u.Username as ChangedByUsername,
                                    h.ChangedAt
                                  FROM UserStatusHistory h
                                  LEFT JOIN UserStatusReason r ON h.ReasonCode = r.ReasonCode
                                  LEFT JOIN User u ON h.ChangedByUserID = u.UserID
                                  WHERE h.UserID = @UserId
                                  ORDER BY h.ChangedAt DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var record = new
                                {
                                    HistoryID = reader.GetInt32("HistoryID"),
                                    FromStatus = reader.IsDBNull(reader.GetOrdinal("FromStatus")) 
                                        ? null 
                                        : reader.GetString("FromStatus"),
                                    ToStatus = reader.GetString("ToStatus"),
                                    ReasonCode = reader.IsDBNull(reader.GetOrdinal("ReasonCode")) 
                                        ? null 
                                        : reader.GetString("ReasonCode"),
                                    ReasonName = reader.IsDBNull(reader.GetOrdinal("ReasonName")) 
                                        ? null 
                                        : reader.GetString("ReasonName"),
                                    ReasonNote = reader.IsDBNull(reader.GetOrdinal("ReasonNote")) 
                                        ? null 
                                        : reader.GetString("ReasonNote"),
                                    ExpiresAt = reader.IsDBNull(reader.GetOrdinal("ExpiresAt")) 
                                        ? (DateTime?)null 
                                        : reader.GetDateTime("ExpiresAt"),
                                    ChangedByUserID = reader.IsDBNull(reader.GetOrdinal("ChangedByUserID")) 
                                        ? (int?)null 
                                        : reader.GetInt32("ChangedByUserID"),
                                    ChangedByUsername = reader.IsDBNull(reader.GetOrdinal("ChangedByUsername")) 
                                        ? null 
                                        : reader.GetString("ChangedByUsername"),
                                    ChangedAt = reader.GetDateTime("ChangedAt")
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
                        COUNT(CASE WHEN u.Role = 'student' THEN 1 END) as TotalStudents,
                        COUNT(CASE WHEN u.Role = 'student' AND u.Status = 'active' THEN 1 END) as ActiveStudents,
                        COUNT(CASE WHEN u.Role = 'student' AND up.CreatedAt >= DATE_FORMAT(NOW(), '%Y-%m-01') THEN 1 END) as NewThisMonth,
                        COUNT(CASE WHEN u.Role = 'student' AND u.Status = 'active' THEN 1 END) as ActiveLearners,
                        COUNT(CASE WHEN u.Role = 'student' AND u.Status = 'inactive' THEN 1 END) as InactiveLong
                    FROM User u
                    LEFT JOIN UserProfile up ON u.UserID = up.UserID";

                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var stats = new
                            {
                                TotalStudents = reader.GetInt32("TotalStudents"),
                                ActiveStudents = reader.GetInt32("ActiveStudents"),
                                NewThisMonth = reader.GetInt32("NewThisMonth"),
                                ActiveLearners = reader.GetInt32("ActiveLearners"),
                                InactiveLong = reader.GetInt32("InactiveLong")
                            };

                            _logger.LogInformation("Retrieved user statistics");
                            return Ok(stats);
                        }
                        else
                        {
                            return Ok(new
                            {
                                TotalStudents = 0,
                                ActiveStudents = 0,
                                NewThisMonth = 0,
                                ActiveLearners = 0,
                                InactiveLong = 0
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
