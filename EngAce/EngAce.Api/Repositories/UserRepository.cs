using Dapper;
using Entities;
using Microsoft.Data.SqlClient;
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

        private SqlConnection GetConnection()
        {
            _logger.LogInformation("🔗 Opening database connection...");
            return new SqlConnection(_connectionString);
        }

        private string MaskPassword(string connectionString)
        {
            var parts = connectionString.Split(';');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                {
                    parts[i] = "Password=***";
                }
            }
            return string.Join(";", parts);
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM [User] WHERE UserID = @UserID";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserID = userId });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            _logger.LogInformation($"📧 Searching user by email: {email}");
            using var connection = GetConnection();
            var sql = "SELECT * FROM [User] WHERE Email = @Email";
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
            _logger.LogInformation($"✅ User found: {user != null}");
            return user;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            _logger.LogInformation($"👤 Searching user by username: {username}");
            using var connection = GetConnection();
            var sql = "SELECT * FROM [User] WHERE Username = @Username";
            var user = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
            _logger.LogInformation($"✅ User found: {user != null}");
            return user;
        }

        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM [User] WHERE GoogleID = @GoogleID";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { GoogleID = googleId });
        }

        public async Task<User?> GetByFacebookIdAsync(string facebookId)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM [User] WHERE FacebookID = @FacebookID";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { FacebookID = facebookId });
        }

        public async Task<User> CreateAsync(User user)
        {
            _logger.LogInformation("➕ Creating new user: {Email}", user.Email);
            using var connection = GetConnection();
            var sql = @"
                INSERT INTO [User] (Email, Username, FullName, PasswordHash, Phone, Avatar, 
                                   Role, Status, EmailVerified, GoogleID, FacebookID, 
                                   CreatedAt, UpdatedAt)
                VALUES (@Email, @Username, @FullName, @PasswordHash, @Phone, @Avatar,
                        @Role, @Status, @EmailVerified, @GoogleID, @FacebookID,
                        @CreatedAt, @UpdatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            
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
                UPDATE [User] 
                SET Email = @Email, 
                    Username = @Username, 
                    FullName = @FullName, 
                    PasswordHash = @PasswordHash,
                    Phone = @Phone, 
                    Avatar = @Avatar, 
                    Role = @Role, 
                    Status = @Status,
                    EmailVerified = @EmailVerified, 
                    GoogleID = @GoogleID, 
                    FacebookID = @FacebookID,
                    LastLoginAt = @LastLoginAt, 
                    UpdatedAt = @UpdatedAt,
                    ResetToken = @ResetToken,
                    ResetTokenExpires = @ResetTokenExpires
                WHERE UserID = @UserID";
            
            await connection.ExecuteAsync(sql, user);
            return user;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using var connection = GetConnection();
            var sql = "SELECT COUNT(1) FROM [User] WHERE Email = @Email";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            using var connection = GetConnection();
            var sql = "SELECT COUNT(1) FROM [User] WHERE Username = @Username";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
            return count > 0;
        }
    }
}
