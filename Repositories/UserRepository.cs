using Dapper;
using Microsoft.Data.SqlClient;
using NotificationService.Models;

namespace NotificationService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("NotificationDatabase")
                ?? throw new InvalidOperationException("Connection string 'NotificationDatabase' is not configured.");
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            const string sql = """
                SELECT * FROM Users
                WHERE Id = @userId;
            """;

            using var conn = new SqlConnection(_connectionString);
            return await conn.QuerySingleOrDefaultAsync<User>(sql, new { userId });
        }

        public async Task<User> CreateUserAsync(User user)
        {
            const string sql = """
                INSERT INTO Users (Id, Email, PhoneNumber)
                VALUES (@Id, @Email, @PhoneNumber);
            """;

            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(sql, user);
            return user;
        }

        public async Task<User> GetOrCreateUserAsync(string userId, string? email = null, string? phoneNumber = null)
        {
            var existingUser = await GetUserByIdAsync(userId);
            if (existingUser != null)
            {
                // If user exists but new email/phone are provided, update them
                if (!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(phoneNumber))
                {
                    const string updateSql = """
                        UPDATE Users 
                        SET Email = COALESCE(@Email, Email), 
                            PhoneNumber = COALESCE(@PhoneNumber, PhoneNumber)
                        WHERE Id = @Id;
                    """;

                    using var conn = new SqlConnection(_connectionString);
                    await conn.ExecuteAsync(updateSql, new { Id = userId, Email = email, PhoneNumber = phoneNumber });
                    
                    // Return updated user
                    return await GetUserByIdAsync(userId) ?? existingUser;
                }
                return existingUser;
            }

            var newUser = new User
            {
                Id = userId,
                Email = email,
                PhoneNumber = phoneNumber
            };

            return await CreateUserAsync(newUser);
        }

        public async Task<(string? Email, string? PhoneNumber)> GetUserContactAsync(string userId)
        {
            const string sql = """
                SELECT Email, PhoneNumber FROM Users
                WHERE Id = @userId;
            """;

            using var conn = new SqlConnection(_connectionString);
            var user = await conn.QuerySingleOrDefaultAsync<User>(sql, new { userId });
            
            return user != null ? (user.Email, user.PhoneNumber) : (null, null);
        }

        public async Task<bool> UpdateUserAsync(string userId, string? email = null, string? phoneNumber = null)
        {
            const string sql = """
                UPDATE Users 
                SET Email = COALESCE(@Email, Email), 
                    PhoneNumber = COALESCE(@PhoneNumber, PhoneNumber)
                WHERE Id = @Id;
            """;

            using var conn = new SqlConnection(_connectionString);
            var result = await conn.ExecuteAsync(sql, new { Id = userId, Email = email, PhoneNumber = phoneNumber });
            return result > 0;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            const string sql = """
                DELETE FROM Users
                WHERE Id = @userId;
            """;

            using var conn = new SqlConnection(_connectionString);
            var result = await conn.ExecuteAsync(sql, new { userId });
            return result > 0;
        }
    }
}
