using Dapper;
using Microsoft.Data.SqlClient;
using NotificationService.Models;
using NotificationService.Interfaces;
using System.Data;

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


        public async Task<IEnumerable<User?>> GetAllUserAsync()
        {
            var users = new List<User>();

            using var conn = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetAllUsers", conn);

            command.CommandType= CommandType.StoredProcedure;

            await conn.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int emailIdx = reader.GetOrdinal("Email");
                int phoneNumberIdx = reader.GetOrdinal("PhoneNumber");
                
                users.Add(new User
                {
                    Id = reader.GetString("Id"),
                    Email = reader.IsDBNull(emailIdx) ? null : reader.GetString(emailIdx),
                    PhoneNumber = reader.IsDBNull(phoneNumberIdx) ? null : reader.GetString(phoneNumberIdx)
                });
            }

            return users;
        }


        public async Task<User?> GetUserByIdAsync(string userId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetUserById", conn);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", userId);
            
            await conn.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                int emailIdx = reader.GetOrdinal("Email");
                int phoneNumberIdx = reader.GetOrdinal("PhoneNumber");
                
                return new User
                {
                    Id = reader.GetString("Id"),
                    Email = reader.IsDBNull(emailIdx) ? null : reader.GetString(emailIdx),
                    PhoneNumber = reader.IsDBNull(phoneNumberIdx) ? null : reader.GetString(phoneNumberIdx)
                };
            }

            return null;
        }


        public async Task<(string? Email, string? PhoneNumber)?> GetUserContactAsync(string userId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetUserContact", conn);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", userId);

            await conn.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            int emailIdx = reader.GetOrdinal("Email");
            int phoneNumberIdx = reader.GetOrdinal("PhoneNumber");

            return(
                reader.IsDBNull(emailIdx) ? null : reader.GetString(emailIdx),
                reader.IsDBNull(phoneNumberIdx) ? null : reader.GetString(phoneNumberIdx)
            );
        }


        public async Task<User?> CreateUserAsync(User user)
        {
            using var conn = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_CreateUser", conn);
            
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", user.Id);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);

            await conn.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = user.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                };
            }
            
            return null;
        }


        public async Task<User> GetOrCreateUserAsync(string userId, string? email = null, string? phoneNumber = null)
        {
            var existingUser = await GetUserByIdAsync(userId);
            if (existingUser != null)
                return existingUser;
            
            var user = new User
            {
                Id = userId,
                Email = email,
                PhoneNumber = phoneNumber
            };

            return await CreateUserAsync(user);
        }

    
        public async Task<bool> UpdateUserAsync(string userId, string? email = null, string? phoneNumber = null)
        {
            using var conn = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_UpdateUser", conn);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", userId);
            command.Parameters.AddWithValue("@Email", email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PhoneNumber", phoneNumber ?? (object)DBNull.Value);

            await conn.OpenAsync();

            int affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows > 0; 
        }


        public async Task<bool> DeleteUserAsync(string userId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_DeleteUser", conn);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", userId);

            await conn.OpenAsync();

            int affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows > 0; 
        }
    }
}
