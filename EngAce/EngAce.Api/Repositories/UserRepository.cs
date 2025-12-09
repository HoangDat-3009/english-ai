using Dapper;
using Entities;
using MySqlConnector;
using System.Threading.Tasks;

namespace EngAce.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("Connection string not found");
            _logger = logger;
            
            _logger.LogInformation($"🔌 Database Connection String: {MaskPassword(_connectionString)}");
        }

        private MySqlConnection GetConnection()
        {
            _logger.LogInformation("🔗 Opening MySQL database connection...");
            return new MySqlConnection(_connectionString);
        }

        private string MaskPassword(string connectionString)
        {
            var parts = connectionString.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase) ||
                    parts[i].Trim().StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
                {
                    var key = parts[i].Split('=')[0];
                    parts[i] = $"{key}=***";
                }
            }
            return string.Join(";", parts);
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM users WHERE id = @UserID";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserID = userId });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            _logger.LogInformation($"📧 Searching user by email: {email}");
            using var connection = GetConnection();
            var sql = "SELECT * FROM users WHERE email = @Email";
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
            _logger.LogInformation($"✅ User found: {user != null}");
            return user;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            _logger.LogInformation($"👤 Searching user by username: {username}");
            using var connection = GetConnection();
            var sql = "SELECT * FROM users WHERE username = @Username";
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
            _logger.LogInformation($"✅ User found: {user != null}");
            return user;
        }

        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM users WHERE google_id = @GoogleID";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { GoogleID = googleId });
        }

        public async Task<User?> GetByFacebookIdAsync(string facebookId)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM users WHERE facebook_id = @FacebookID";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { FacebookID = facebookId });
        }

        public async Task<User> CreateAsync(User user)
        {
            _logger.LogInformation("➕ Creating new user: {Email}", user.Email);
            using var connection = GetConnection();
            
            // Generate username if not provided (for OAuth users)
            if (string.IsNullOrWhiteSpace(user.Username))
            {
                user.Username = $"user_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            }
            
            // Set empty password hash if not provided (for OAuth users)
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                user.PasswordHash = string.Empty;
            }
            
            var sql = @"
                INSERT INTO users (email, username, full_name, password_hash, phone, avatar_url, 
                                   status, account_type, role, google_id, facebook_id, 
                                   created_at, updated_at)
                VALUES (@Email, @Username, @FullName, @PasswordHash, @Phone, @Avatar,
                        @Status, @AccountType, @Role, @GoogleID, @FacebookID,
                        @CreatedAt, @UpdatedAt);
                SELECT LAST_INSERT_ID();";
            
            var userId = await connection.ExecuteScalarAsync<int>(sql, user);
            user.UserID = userId;
            _logger.LogInformation("✅ User created with ID: {UserId}", userId);
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            using var connection = GetConnection();
            user.UpdatedAt = DateTime.UtcNow;
            
            var sql = @"
                UPDATE users 
                SET email = @Email, 
                    username = @Username, 
                    full_name = @FullName, 
                    password_hash = @PasswordHash,
                    phone = @Phone, 
                    avatar_url = @Avatar, 
                    status = @Status,
                    account_type = @AccountType,
                    role = @Role,
                    google_id = @GoogleID, 
                    facebook_id = @FacebookID,
                    last_active_at = @LastLoginAt, 
                    updated_at = @UpdatedAt
                WHERE id = @UserID";
            
            await connection.ExecuteAsync(sql, user);
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using var connection = GetConnection();
            var sql = "SELECT COUNT(1) FROM users WHERE email = @Email";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            using var connection = GetConnection();
            var sql = "SELECT COUNT(1) FROM users WHERE username = @Username";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
            return count > 0;
        }
    }
}
