using Dapper;
using Entities;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace EngAce.Api.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("Connection string not found");
        }

        private SqlConnection GetConnection() => new SqlConnection(_connectionString);

        public async Task<User?> GetByIdAsync(int userId)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM Users WHERE UserID = @UserID";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserID = userId });
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM Users WHERE Email = @Email";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM Users WHERE Username = @Username";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<User?> GetByGoogleIdAsync(string googleId)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM Users WHERE GoogleID = @GoogleID";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { GoogleID = googleId });
        }

        public async Task<User?> GetByFacebookIdAsync(string facebookId)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM Users WHERE FacebookID = @FacebookID";
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { FacebookID = facebookId });
        }

        public async Task<User> CreateAsync(User user)
        {
            using var connection = GetConnection();
            var sql = @"
                INSERT INTO Users (Email, Username, FullName, PasswordHash, Phone, Avatar, 
                                   Role, Status, EmailVerified, GoogleID, FacebookID, 
                                   CreatedAt, UpdatedAt)
                VALUES (@Email, @Username, @FullName, @PasswordHash, @Phone, @Avatar,
                        @Role, @Status, @EmailVerified, @GoogleID, @FacebookID,
                        @CreatedAt, @UpdatedAt);
                SELECT LAST_INSERT_ID();";
            
            var userId = await connection.ExecuteScalarAsync<int>(sql, user);
            user.UserID = userId;
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            using var connection = GetConnection();
            user.UpdatedAt = DateTime.UtcNow;
            
            var sql = @"
                UPDATE Users 
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
            var sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
            return count > 0;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            using var connection = GetConnection();
            var sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
            return count > 0;
        }
    }
}
