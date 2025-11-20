using Entities;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Linq;

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
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM users", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        statistics.TotalUsers = Convert.ToInt32(result);
                    }

                    // Get active users (users with status = 'active')
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM users WHERE status = 'active'", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        statistics.ActiveUsers = Convert.ToInt32(result);
                    }

                    // Get new users this month (users registered in current month)
                    using (var command = new MySqlCommand(
                        @"SELECT COUNT(*) FROM users
                          WHERE YEAR(created_at) = YEAR(CURDATE()) 
                          AND MONTH(created_at) = MONTH(CURDATE())", 
                        connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        statistics.NewUsersThisMonth = Convert.ToInt32(result);
                    }

                    // Get total tests
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM tests", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        statistics.TotalTests = Convert.ToInt32(result);
                    }

                    // Get total exercises
                    using (var command = new MySqlCommand("SELECT COUNT(*) FROM exercises", connection))
                    {
                        var result = await command.ExecuteScalarAsync();
                        statistics.TotalExercises = Convert.ToInt32(result);
                    }

                    // Get total completions (exercise + test)
                    try
                    {
                        using (var command = new MySqlCommand(
                            "SELECT (SELECT COUNT(*) FROM exercise_completions) + (SELECT COUNT(*) FROM test_completions) as total", 
                            connection))
                        {
                            var result = await command.ExecuteScalarAsync();
                            statistics.TotalCompletions = Convert.ToInt32(result);
                        }
                    }
                    catch (MySqlException ex) when (ex.Message.Contains("doesn't exist"))
                    {
                        _logger.LogWarning("Completions table not found, setting total completions to 0");
                        statistics.TotalCompletions = 0;
                    }

                    // Get total revenue (completed payments only)
                    try
                    {
                        using (var command = new MySqlCommand(
                            "SELECT COALESCE(SUM(amount), 0) FROM payments WHERE status = 'completed'", 
                            connection))
                        {
                            var result = await command.ExecuteScalarAsync();
                            statistics.TotalRevenue = Convert.ToDouble(result);
                        }
                    }
                    catch (MySqlException ex) when (ex.Message.Contains("doesn't exist"))
                    {
                        _logger.LogWarning("payments table not found, setting revenue to 0");
                        statistics.TotalRevenue = 0;
                    }

                    // Get revenue this month (completed payments in current month)
                    try
                    {
                        using (var command = new MySqlCommand(
                            @"SELECT COALESCE(SUM(amount), 0) FROM payments 
                              WHERE status = 'completed' 
                              AND YEAR(created_at) = YEAR(CURDATE()) 
                              AND MONTH(created_at) = MONTH(CURDATE())", 
                            connection))
                        {
                            var result = await command.ExecuteScalarAsync();
                            statistics.RevenueThisMonth = Convert.ToDouble(result);
                        }
                    }
                    catch (MySqlException ex) when (ex.Message.Contains("doesn't exist"))
                    {
                        _logger.LogWarning("payments table not found, setting revenue this month to 0");
                        statistics.RevenueThisMonth = 0;
                    }

                    // Get pending payments count
                    try
                    {
                        using (var command = new MySqlCommand(
                            "SELECT COUNT(*) FROM payments WHERE status = 'pending'", 
                            connection))
                        {
                            var result = await command.ExecuteScalarAsync();
                            statistics.PendingPayments = Convert.ToInt32(result);
                        }
                    }
                    catch (MySqlException ex) when (ex.Message.Contains("doesn't exist"))
                    {
                        _logger.LogWarning("payments table not found, setting pending payments to 0");
                        statistics.PendingPayments = 0;
                    }

                    // Get total revenue (completed payments only)
                    try
                    {
                        using (var command = new MySqlCommand(
                            "SELECT COALESCE(SUM(amount), 0) FROM payments WHERE status = 'completed'", 
                            connection))
                        {
                            var result = await command.ExecuteScalarAsync();
                            statistics.TotalRevenue = Convert.ToDouble(result);
                        }
                    }
                    catch (MySqlException ex) when (ex.Message.Contains("doesn't exist"))
                    {
                        _logger.LogWarning("payments table not found, setting revenue to 0");
                        statistics.TotalRevenue = 0;
                    }

                    // Get revenue this month (completed payments in current month)
                    try
                    {
                        using (var command = new MySqlCommand(
                            @"SELECT COALESCE(SUM(amount), 0) FROM payments 
                              WHERE status = 'completed' 
                              AND YEAR(created_at) = YEAR(CURDATE()) 
                              AND MONTH(created_at) = MONTH(CURDATE())", 
                            connection))
                        {
                            var result = await command.ExecuteScalarAsync();
                            statistics.RevenueThisMonth = Convert.ToDouble(result);
                        }
                    }
                    catch (MySqlException ex) when (ex.Message.Contains("doesn't exist"))
                    {
                        _logger.LogWarning("payments table not found, setting revenue this month to 0");
                        statistics.RevenueThisMonth = 0;
                    }

                    // Get pending payments count
                    try
                    {
                        using (var command = new MySqlCommand(
                            "SELECT COUNT(*) FROM payments WHERE status = 'pending'", 
                            connection))
                        {
                            var result = await command.ExecuteScalarAsync();
                            statistics.PendingPayments = Convert.ToInt32(result);
                        }
                    }
                    catch (MySqlException ex) when (ex.Message.Contains("doesn't exist"))
                    {
                        _logger.LogWarning("payments table not found, setting pending payments to 0");
                        statistics.PendingPayments = 0;
                    }
                }

                _logger.LogInformation("System statistics retrieved successfully: {TotalUsers} total users, {ActiveUsers} active, {NewUsers} new this month, {Tests} tests, {Exercises} exercises, {Completions} completions, {Revenue} VND total revenue, {RevenueMonth} VND revenue this month, {PendingPayments} pending payments", 
                    statistics.TotalUsers, statistics.ActiveUsers, statistics.NewUsersThisMonth, statistics.TotalTests, statistics.TotalExercises, statistics.TotalCompletions, statistics.TotalRevenue, statistics.RevenueThisMonth, statistics.PendingPayments);

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
        /// Get user count by account type
        /// </summary>
        /// <returns>User counts by account type</returns>
        [HttpGet("users-by-account-type")]
        public async Task<ActionResult<Dictionary<string, int>>> GetUsersByAccountType()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                var usersByAccountType = new Dictionary<string, int>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand("SELECT account_type, COUNT(*) as Count FROM users GROUP BY account_type", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var accountType = reader.GetString("account_type");
                                var count = reader.GetInt32("Count");
                                usersByAccountType[accountType] = count;
                            }
                        }
                    }
                }

                _logger.LogInformation("User statistics by account type retrieved successfully");

                return Ok(usersByAccountType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user statistics by account type");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Get user growth data for the last 12 months
        /// </summary>
        /// <returns>List of user growth data by month</returns>
        [HttpGet("user-growth")]
        public async Task<ActionResult<List<UserGrowthData>>> GetUserGrowth()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                var userGrowthData = new List<UserGrowthData>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Get user growth for last 12 months
                    var query = @"
                        SELECT 
                            DATE_FORMAT(created_at, '%Y-%m') as YearMonth,
                            MONTH(created_at) as MonthNum,
                            COUNT(DISTINCT id) as NewUsers,
                            COUNT(DISTINCT CASE WHEN status = 'active' THEN id END) as ActiveUsers
                        FROM users
                        WHERE created_at >= DATE_SUB(CURDATE(), INTERVAL 12 MONTH)
                        GROUP BY DATE_FORMAT(created_at, '%Y-%m'), MONTH(created_at)
                        ORDER BY YearMonth ASC
                        LIMIT 12";

                    try
                    {
                        using (var command = new MySqlCommand(query, connection))
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var monthNum = reader.GetInt32("MonthNum");
                                    var newUsers = reader.GetInt32("NewUsers");
                                    var activeUsers = reader.GetInt32("ActiveUsers");

                                    userGrowthData.Add(new UserGrowthData
                                    {
                                        Month = $"T{monthNum}",
                                        NewUsers = newUsers,
                                        ActiveUsers = activeUsers
                                    });
                                }
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        _logger.LogError(ex, "Error executing user growth query");
                        // Return empty data if query fails
                        return Ok(new List<UserGrowthData>());
                    }

                    // If we have less than 12 months of data, fill in missing months with zeros
                    if (userGrowthData.Count < 12)
                    {
                        var existingMonths = userGrowthData.Select(d => d.Month).ToHashSet();
                        for (int i = 1; i <= 12; i++)
                        {
                            var monthLabel = $"T{i}";
                            if (!existingMonths.Contains(monthLabel))
                            {
                                userGrowthData.Add(new UserGrowthData
                                {
                                    Month = monthLabel,
                                    NewUsers = 0,
                                    ActiveUsers = 0
                                });
                            }
                        }
                        // Sort by month number
                        userGrowthData = userGrowthData
                            .OrderBy(d => int.Parse(d.Month.Replace("T", "")))
                            .ToList();
                    }
                }

                _logger.LogInformation("User growth data retrieved successfully: {Count} months", userGrowthData.Count);

                return Ok(userGrowthData);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving user growth data");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user growth data");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Get user growth data for the last 12 months
        /// </summary>
        /// <returns>List of user growth data by month</returns>
        [HttpGet("user-growth")]
        public async Task<ActionResult<List<UserGrowthData>>> GetUserGrowth()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                var userGrowthData = new List<UserGrowthData>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Get user growth for last 12 months
                    var query = @"
                        SELECT 
                            DATE_FORMAT(created_at, '%Y-%m') as YearMonth,
                            MONTH(created_at) as MonthNum,
                            COUNT(DISTINCT id) as NewUsers,
                            COUNT(DISTINCT CASE WHEN status = 'active' THEN id END) as ActiveUsers
                        FROM users
                        WHERE created_at >= DATE_SUB(CURDATE(), INTERVAL 12 MONTH)
                        GROUP BY DATE_FORMAT(created_at, '%Y-%m'), MONTH(created_at)
                        ORDER BY YearMonth ASC
                        LIMIT 12";

                    try
                    {
                        using (var command = new MySqlCommand(query, connection))
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var monthNum = reader.GetInt32("MonthNum");
                                    var newUsers = reader.GetInt32("NewUsers");
                                    var activeUsers = reader.GetInt32("ActiveUsers");

                                    userGrowthData.Add(new UserGrowthData
                                    {
                                        Month = $"T{monthNum}",
                                        NewUsers = newUsers,
                                        ActiveUsers = activeUsers
                                    });
                                }
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        _logger.LogError(ex, "Error executing user growth query");
                        // Return empty data if query fails
                        return Ok(new List<UserGrowthData>());
                    }

                    // If we have less than 12 months of data, fill in missing months with zeros
                    if (userGrowthData.Count < 12)
                    {
                        var existingMonths = userGrowthData.Select(d => d.Month).ToHashSet();
                        for (int i = 1; i <= 12; i++)
                        {
                            var monthLabel = $"T{i}";
                            if (!existingMonths.Contains(monthLabel))
                            {
                                userGrowthData.Add(new UserGrowthData
                                {
                                    Month = monthLabel,
                                    NewUsers = 0,
                                    ActiveUsers = 0
                                });
                            }
                        }
                        // Sort by month number
                        userGrowthData = userGrowthData
                            .OrderBy(d => int.Parse(d.Month.Replace("T", "")))
                            .ToList();
                    }
                }

                _logger.LogInformation("User growth data retrieved successfully: {Count} months", userGrowthData.Count);

                return Ok(userGrowthData);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving user growth data");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user growth data");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Get revenue and payment data for the last 12 months
        /// </summary>
        /// <returns>List of revenue and payment data by month</returns>
        [HttpGet("revenue-payment")]
        public async Task<ActionResult<List<RevenuePaymentData>>> GetRevenuePayment()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                var revenuePaymentData = new List<RevenuePaymentData>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Get revenue and payment data for last 12 months
                    var query = @"
                        SELECT 
                            DATE_FORMAT(p.created_at, '%Y-%m') as YearMonth,
                            MONTH(p.created_at) as MonthNum,
                            COUNT(*) as TotalPayments,
                            SUM(CASE WHEN p.status = 'completed' THEN p.amount ELSE 0 END) as Revenue,
                            SUM(CASE WHEN p.status = 'pending' THEN p.amount ELSE 0 END) as PendingAmount,
                            SUM(CASE WHEN p.status = 'failed' THEN p.amount ELSE 0 END) as FailedAmount
                        FROM payments p
                        WHERE p.created_at >= DATE_SUB(CURDATE(), INTERVAL 12 MONTH)
                        GROUP BY DATE_FORMAT(p.created_at, '%Y-%m'), MONTH(p.created_at)
                        ORDER BY YearMonth ASC
                        LIMIT 12";

                    try
                    {
                        using (var command = new MySqlCommand(query, connection))
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var monthNum = reader.GetInt32("MonthNum");
                                    var totalPayments = reader.GetInt32("TotalPayments");
                                    var revenue = reader.IsDBNull(reader.GetOrdinal("Revenue")) ? 0 : reader.GetDouble("Revenue");
                                    var pendingAmount = reader.IsDBNull(reader.GetOrdinal("PendingAmount")) ? 0 : reader.GetDouble("PendingAmount");
        /// <summary>
        /// Get revenue and payment data for the last 12 months
        /// </summary>
        /// <returns>List of revenue and payment data by month</returns>
        [HttpGet("revenue-payment")]
        public async Task<ActionResult<List<RevenuePaymentData>>> GetRevenuePayment()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("LearningSystemDb");
                if (string.IsNullOrEmpty(connectionString))
                {
                    _logger.LogError("Database connection string is not configured");
                    return StatusCode(500, "Database connection not configured");
                }

                var revenuePaymentData = new List<RevenuePaymentData>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Get revenue and payment data for last 12 months
                    var query = @"
                        SELECT 
                            DATE_FORMAT(p.created_at, '%Y-%m') as YearMonth,
                            MONTH(p.created_at) as MonthNum,
                            COUNT(*) as TotalPayments,
                            SUM(CASE WHEN p.status = 'completed' THEN p.amount ELSE 0 END) as Revenue,
                            SUM(CASE WHEN p.status = 'pending' THEN p.amount ELSE 0 END) as PendingAmount,
                            SUM(CASE WHEN p.status = 'failed' THEN p.amount ELSE 0 END) as FailedAmount
                        FROM payments p
                        WHERE p.created_at >= DATE_SUB(CURDATE(), INTERVAL 12 MONTH)
                        GROUP BY DATE_FORMAT(p.created_at, '%Y-%m'), MONTH(p.created_at)
                        ORDER BY YearMonth ASC
                        LIMIT 12";

                    try
                    {
                        using (var command = new MySqlCommand(query, connection))
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var monthNum = reader.GetInt32("MonthNum");
                                    var totalPayments = reader.GetInt32("TotalPayments");
                                    var revenue = reader.IsDBNull(reader.GetOrdinal("Revenue")) ? 0 : reader.GetDouble("Revenue");
                                    var pendingAmount = reader.IsDBNull(reader.GetOrdinal("PendingAmount")) ? 0 : reader.GetDouble("PendingAmount");
                                    var failedAmount = reader.IsDBNull(reader.GetOrdinal("FailedAmount")) ? 0 : reader.GetDouble("FailedAmount");

                                    revenuePaymentData.Add(new RevenuePaymentData
                                    {
                                        Month = $"T{monthNum}",
                                        Revenue = revenue,
                                        TotalPayments = totalPayments,
                                        PendingAmount = pendingAmount,
                                        FailedAmount = failedAmount
                                    });
                                }
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        _logger.LogError(ex, "Error executing revenue payment query");
                        // Return empty data if query fails
                        return Ok(new List<RevenuePaymentData>());
                    }

                    // If we have less than 12 months of data, fill in missing months with zeros
                    if (revenuePaymentData.Count < 12)
                    {
                        var existingMonths = revenuePaymentData.Select(d => d.Month).ToHashSet();
                        for (int i = 1; i <= 12; i++)
                        {
                            var monthLabel = $"T{i}";
                            if (!existingMonths.Contains(monthLabel))
                            {
                                revenuePaymentData.Add(new RevenuePaymentData
                                {
                                    Month = monthLabel,
                                    Revenue = 0,
                                    TotalPayments = 0,
                                    PendingAmount = 0,
                                    FailedAmount = 0
                                });
                            }
                        }
                        // Sort by month number
                        revenuePaymentData = revenuePaymentData
                            .OrderBy(d => int.Parse(d.Month.Replace("T", "")))
                            .ToList();
                    }
                }

                _logger.LogInformation("Revenue payment data retrieved successfully: {Count} months", revenuePaymentData.Count);

                return Ok(revenuePaymentData);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving revenue payment data");
                return StatusCode(500, $"Database error: {ex.Message}");
                _logger.LogError(ex, "An error occurred while retrieving revenue payment data");
                                    var failedAmount = reader.IsDBNull(reader.GetOrdinal("FailedAmount")) ? 0 : reader.GetDouble("FailedAmount");

                                    revenuePaymentData.Add(new RevenuePaymentData
                                    {
                                        Month = $"T{monthNum}",
                                        Revenue = revenue,
                                        TotalPayments = totalPayments,
                                        PendingAmount = pendingAmount,
                                        FailedAmount = failedAmount
                                    });
                                }
                            }
                        }
                    }
                    catch (MySqlException ex)
                    {
                        _logger.LogError(ex, "Error executing revenue payment query");
                        // Return empty data if query fails
                        return Ok(new List<RevenuePaymentData>());
                    }

                    // If we have less than 12 months of data, fill in missing months with zeros
                    if (revenuePaymentData.Count < 12)
                    {
                        var existingMonths = revenuePaymentData.Select(d => d.Month).ToHashSet();
                        for (int i = 1; i <= 12; i++)
                        {
                            var monthLabel = $"T{i}";
                            if (!existingMonths.Contains(monthLabel))
                            {
                                revenuePaymentData.Add(new RevenuePaymentData
                                {
                                    Month = monthLabel,
                                    Revenue = 0,
                                    TotalPayments = 0,
                                    PendingAmount = 0,
                                    FailedAmount = 0
                                });
                            }
                        }
                        // Sort by month number
                        revenuePaymentData = revenuePaymentData
                            .OrderBy(d => int.Parse(d.Month.Replace("T", "")))
                            .ToList();
                    }
                }

                _logger.LogInformation("Revenue payment data retrieved successfully: {Count} months", revenuePaymentData.Count);

                return Ok(revenuePaymentData);
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, "MySQL error occurred while retrieving revenue payment data");
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving revenue payment data");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
