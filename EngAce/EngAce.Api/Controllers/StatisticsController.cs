using Entities;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace EngAce.Api.Controllers
{
    /// <summary>
    /// Controller for system statistics endpoints
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StatisticsController> _logger;

        /// <summary>
        /// Constructor for StatisticsController
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        /// <param name="logger">Logger instance</param>
        public StatisticsController(IConfiguration configuration, ILogger<StatisticsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Get system-wide statistics including total users and tests
        /// </summary>
        /// <returns>System statistics</returns>
        [HttpGet]
        public async Task<ActionResult<SystemStatistics>> GetSystemStatistics()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                var statistics = new SystemStatistics();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Get total users
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM User", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        statistics.TotalUsers = Convert.ToInt32(result);
                    }

                    // Get total tests (exams)
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM Exam", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        statistics.TotalTests = Convert.ToInt32(result);
                    }

                    // Get total exercises
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM Exercise", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        statistics.TotalExercises = Convert.ToInt32(result);
                    }

                    // Get total completions
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM Completion", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        statistics.TotalCompletions = Convert.ToInt32(result);
                    }
                }

                _logger.LogInformation("System statistics retrieved successfully: {Users} users, {Tests} tests, {Exercises} exercises, {Completions} completions", 
                    statistics.TotalUsers, statistics.TotalTests, statistics.TotalExercises, statistics.TotalCompletions);

                return Ok(statistics);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving statistics");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving statistics");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Get user count by role
        /// </summary>
        /// <returns>User counts by role</returns>
        [HttpGet("users-by-role")]
        public async Task<ActionResult<Dictionary<string, int>>> GetUsersByRole()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                var usersByRole = new Dictionary<string, int>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand("SELECT Role, COUNT(*) as Count FROM User GROUP BY Role", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var role = reader.GetString("Role");
                                var count = reader.GetInt32("Count");
                                usersByRole[role] = count;
                            }
                        }
                    }
                }

                _logger.LogInformation("User statistics by role retrieved successfully");

                return Ok(usersByRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user statistics by role");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
