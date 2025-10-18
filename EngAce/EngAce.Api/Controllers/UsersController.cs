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

                var users = new List<User>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"SELECT UserID, Username, Email, Phone, Role, Status 
                                  FROM User 
                                  ORDER BY UserID DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
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

                var users = new List<User>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"SELECT UserID, Username, Email, Phone, Role, Status 
                                  FROM User 
                                  WHERE Role = @Role
                                  ORDER BY UserID DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Role", role);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
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
        /// Update user status (active, inactive, banned)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="request">Request containing new status value</param>
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

                    // Check if user exists
                    var checkQuery = "SELECT COUNT(*) FROM User WHERE UserID = @UserId";
                    using (var checkCommand = new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@UserId", id);
                        var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                        
                        if (count == 0)
                        {
                            _logger.LogWarning("Attempted to update status for non-existent user {UserId}", id);
                            return NotFound($"User with ID {id} not found");
                        }
                    }

                    // Update status
                    var updateQuery = "UPDATE User SET Status = @Status WHERE UserID = @UserId";
                    using (var updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@Status", request.Status.ToLower());
                        updateCommand.Parameters.AddWithValue("@UserId", id);
                        
                        var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                        
                        if (rowsAffected > 0)
                        {
                            _logger.LogInformation("Successfully updated user {UserId} status to {Status}", id, request.Status);
                            return Ok(new { 
                                message = $"User status updated to '{request.Status}' successfully",
                                userId = id,
                                newStatus = request.Status.ToLower()
                            });
                        }
                        else
                        {
                            _logger.LogError("Failed to update user {UserId} status", id);
                            return StatusCode(500, "Failed to update user status");
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
    }
}
