// ============================================================================
// FoodDeliverySystem.Application/Services/AuthService.cs
// ============================================================================
// IMPORTANT: This service uses ONLY STORED PROCEDURES - NO EF Core queries
// All database operations execute stored procedures using ADO.NET
// 
// STORED PROCEDURES USED IN THIS SERVICE:
// 1. SP_LoginUser          - Authenticates user by email
// 2. SP_RegisterCustomer   - Registers new customer
// 3. SP_CreateAdmin        - Creates admin account
// 4. SP_CreateRider        - Creates rider account
// 5. SP_DeleteAccount      - Deletes user account
// 6. SP_GetAllUsers        - Retrieves all users
// 7. SP_CheckEmailExists   - Checks email uniqueness
// 8. SP_ResetPassword      - Resets user password (Forgot Password)
// 9. SP_ChangePassword     - Changes password for authenticated user
// 10. SP_GetUserPasswordHash - Gets password hash for verification
// ============================================================================

using FoodDeliverySystem.Application.DTOs;
using FoodDeliverySystem.Application.Interfaces;
using FoodDeliverySystem.Common.Helpers;
using FoodDeliverySystem.Domain.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FoodDeliverySystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly string _connectionString;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IConfiguration configuration, JwtSettings jwtSettings)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
            _jwtSettings = jwtSettings;
        }

        // ========== AUTHENTICATION METHODS ==========

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SP_LoginUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", loginDto.Email);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            throw new UnauthorizedAccessException("Invalid email or password");
                        }

                        var passwordHash = reader["PasswordHash"].ToString() ?? "";

                        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, passwordHash))
                        {
                            throw new UnauthorizedAccessException("Invalid email or password");
                        }

                        var userId = Guid.Parse(reader["Id"].ToString() ?? Guid.Empty.ToString());
                        var email = reader["Email"].ToString() ?? "";
                        var fullName = reader["FullName"].ToString() ?? "";
                        var role = (UserRole)reader.GetInt32("Role");

                        var token = JwtHelper.GenerateToken(email, role, userId.ToString(), _jwtSettings);

                        Console.WriteLine($"[{DateTime.UtcNow}] User logged in: {email} (Role: {role})");

                        return new AuthResponseDto
                        {
                            Token = token,
                            Email = email,
                            FullName = fullName,
                            Role = role.ToString(),
                            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
                        };
                    }
                }
            }
        }

        public async Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerDto registerDto)
        {
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                throw new ArgumentException("Passwords do not match");
            }

            if (await EmailExistsAsync(registerDto.Email))
            {
                throw new ArgumentException("Email already exists");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SP_RegisterCustomer", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@FullName", registerDto.FullName);
                    command.Parameters.AddWithValue("@Email", registerDto.Email);
                    command.Parameters.AddWithValue("@PasswordHash",
                        BCrypt.Net.BCrypt.HashPassword(registerDto.Password));
                    command.Parameters.AddWithValue("@PhoneNumber", registerDto.PhoneNumber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Address", registerDto.Address ?? (object)DBNull.Value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            throw new Exception("Failed to register customer");
                        }

                        var userId = Guid.Parse(reader["Id"].ToString() ?? Guid.Empty.ToString());
                        var email = reader["Email"].ToString() ?? "";
                        var fullName = reader["FullName"].ToString() ?? "";
                        var role = (UserRole)reader.GetInt32("Role");

                        var token = JwtHelper.GenerateToken(email, role, userId.ToString(), _jwtSettings);

                        Console.WriteLine($"[{DateTime.UtcNow}] Customer self-registered: {email}");

                        return new AuthResponseDto
                        {
                            Token = token,
                            Email = email,
                            FullName = fullName,
                            Role = role.ToString(),
                            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
                        };
                    }
                }
            }
        }

        // ========== ADMIN-ONLY ACCOUNT CREATION ==========

        public async Task<AuthResponseDto> CreateAdminAsync(CreateAdminDto adminDto, UserRole creatorRole, string creatorEmail)
        {
            if (creatorRole != UserRole.SuperAdmin)
            {
                throw new UnauthorizedAccessException(
                    $"Only Super Admin can create Admin accounts. Your role: {creatorRole}");
            }

            if (adminDto.Password != adminDto.ConfirmPassword)
            {
                throw new ArgumentException("Passwords do not match");
            }

            if (await EmailExistsAsync(adminDto.Email))
            {
                throw new ArgumentException("Email already exists");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SP_CreateAdmin", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@FullName", adminDto.FullName);
                    command.Parameters.AddWithValue("@Email", adminDto.Email);
                    command.Parameters.AddWithValue("@PasswordHash",
                        BCrypt.Net.BCrypt.HashPassword(adminDto.Password));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            throw new Exception("Failed to create admin account");
                        }

                        var userId = Guid.Parse(reader["Id"].ToString() ?? Guid.Empty.ToString());
                        var email = reader["Email"].ToString() ?? "";
                        var fullName = reader["FullName"].ToString() ?? "";
                        var role = (UserRole)reader.GetInt32("Role");

                        var token = JwtHelper.GenerateToken(email, role, userId.ToString(), _jwtSettings);

                        Console.WriteLine($"[{DateTime.UtcNow}] Admin account created by {creatorEmail}");
                        Console.WriteLine($"  New Admin: {email} ({adminDto.FullName})");

                        return new AuthResponseDto
                        {
                            Token = token,
                            Email = email,
                            FullName = fullName,
                            Role = role.ToString(),
                            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
                        };
                    }
                }
            }
        }

        public async Task<AuthResponseDto> CreateRiderAsync(CreateRiderDto riderDto, UserRole creatorRole, string creatorEmail)
        {
            if (creatorRole != UserRole.SuperAdmin && creatorRole != UserRole.Admin)
            {
                throw new UnauthorizedAccessException(
                    $"Only Super Admin or Admin can create Rider accounts. Your role: {creatorRole}");
            }

            if (riderDto.Password != riderDto.ConfirmPassword)
            {
                throw new ArgumentException("Passwords do not match");
            }

            if (await EmailExistsAsync(riderDto.Email))
            {
                throw new ArgumentException("Email already exists");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SP_CreateRider", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@FullName", riderDto.FullName);
                    command.Parameters.AddWithValue("@Email", riderDto.Email);
                    command.Parameters.AddWithValue("@PasswordHash",
                        BCrypt.Net.BCrypt.HashPassword(riderDto.Password));
                    command.Parameters.AddWithValue("@ContactNumber", riderDto.ContactNumber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MotorcycleModel", riderDto.MotorcycleModel ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PlateNumber", riderDto.PlateNumber ?? (object)DBNull.Value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            throw new Exception("Failed to create rider account");
                        }

                        var userId = Guid.Parse(reader["Id"].ToString() ?? Guid.Empty.ToString());
                        var email = reader["Email"].ToString() ?? "";
                        var fullName = reader["FullName"].ToString() ?? "";
                        var role = (UserRole)reader.GetInt32("Role");
                        var plateNumber = reader["PlateNumber"].ToString() ?? "";

                        var token = JwtHelper.GenerateToken(email, role, userId.ToString(), _jwtSettings);

                        Console.WriteLine($"[{DateTime.UtcNow}] Rider account created by {creatorEmail}");
                        Console.WriteLine($"  New Rider: {email} ({riderDto.FullName}) - {plateNumber}");

                        return new AuthResponseDto
                        {
                            Token = token,
                            Email = email,
                            FullName = fullName,
                            Role = role.ToString(),
                            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
                        };
                    }
                }
            }
        }

        // ========== ADMIN-ONLY ACCOUNT MANAGEMENT ==========

        public async Task<bool> DeleteAccountAsync(string email, UserRole deleterRole, string deleterEmail)
        {
            if (deleterRole != UserRole.SuperAdmin && deleterRole != UserRole.Admin)
            {
                throw new UnauthorizedAccessException(
                    $"Only Super Admin or Admin can delete accounts. Your role: {deleterRole}");
            }

            if (email == deleterEmail)
            {
                throw new InvalidOperationException("You cannot delete your own account");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SP_DeleteAccount", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", email);

                    var deletedCountParam = new SqlParameter("@DeletedCount", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    var userTypeParam = new SqlParameter("@UserType", SqlDbType.NVarChar, 50)
                    {
                        Direction = ParameterDirection.Output
                    };
                    var userRoleParam = new SqlParameter("@UserRole", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };

                    command.Parameters.Add(deletedCountParam);
                    command.Parameters.Add(userTypeParam);
                    command.Parameters.Add(userRoleParam);

                    await command.ExecuteNonQueryAsync();

                    var deletedCount = (int)deletedCountParam.Value;
                    var userType = userTypeParam.Value.ToString() ?? "Unknown";
                    var deletedUserRole = userRoleParam.Value == DBNull.Value
                        ? (UserRole?)null
                        : (UserRole)(int)userRoleParam.Value;

                    if (deletedCount == 0)
                    {
                        throw new KeyNotFoundException($"Account with email '{email}' not found");
                    }

                    if (deletedUserRole == UserRole.Admin && deleterRole != UserRole.SuperAdmin)
                    {
                        throw new UnauthorizedAccessException("Only Super Admin can delete Admin accounts");
                    }

                    Console.WriteLine($"[{DateTime.UtcNow}] {userType} account deleted by {deleterEmail}");
                    Console.WriteLine($"  Deleted {userType}: {email}");

                    return true;
                }
            }
        }

        public async Task<List<UserInfoDto>> GetAllUsersAsync(UserRole requesterRole)
        {
            if (requesterRole != UserRole.SuperAdmin && requesterRole != UserRole.Admin)
            {
                throw new UnauthorizedAccessException(
                    $"Only Super Admin or Admin can view all users. Your role: {requesterRole}");
            }

            var allUsers = new List<UserInfoDto>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SP_GetAllUsers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var user = new UserInfoDto
                            {
                                Id = Guid.Parse(reader["Id"].ToString() ?? Guid.Empty.ToString()),
                                FullName = reader["FullName"].ToString() ?? "",
                                Email = reader["Email"].ToString() ?? "",
                                Role = ((UserRole)reader.GetInt32("Role")).ToString(),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                IsActive = reader.GetBoolean("IsActive"),
                                ContactNumber = reader["ContactNumber"] == DBNull.Value
                                    ? null : reader["ContactNumber"].ToString(),
                                MotorcycleModel = reader["MotorcycleModel"] == DBNull.Value
                                    ? null : reader["MotorcycleModel"].ToString(),
                                PlateNumber = reader["PlateNumber"] == DBNull.Value
                                    ? null : reader["PlateNumber"].ToString(),
                                PhoneNumber = reader["PhoneNumber"] == DBNull.Value
                                    ? null : reader["PhoneNumber"].ToString(),
                                Address = reader["Address"] == DBNull.Value
                                    ? null : reader["Address"].ToString()
                            };

                            allUsers.Add(user);
                        }
                    }
                }
            }

            return allUsers;
        }

        // ========== PASSWORD MANAGEMENT ==========

        /// <summary>
        /// Reset password (Forgot Password flow) - Public endpoint
        /// STORED PROCEDURE: SP_ResetPassword
        /// </summary>
        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            {
                throw new ArgumentException("Passwords do not match");
            }

            // Check if user exists
            if (!await EmailExistsAsync(resetPasswordDto.Email))
            {
                throw new KeyNotFoundException("Email not found");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // ============================================================================
                // STORED PROCEDURE: SP_ResetPassword
                // Purpose: Reset user password in appropriate table
                // Parameters: @Email, @NewPasswordHash
                // Returns: UpdatedCount (INT), UserType (NVARCHAR)
                // ============================================================================
                using (var command = new SqlCommand("SP_ResetPassword", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", resetPasswordDto.Email);
                    command.Parameters.AddWithValue("@NewPasswordHash",
                        BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var updatedCount = reader.GetInt32(reader.GetOrdinal("UpdatedCount"));
                            var userType = reader["UserType"].ToString() ?? "Unknown";

                            if (updatedCount > 0)
                            {
                                Console.WriteLine($"[{DateTime.UtcNow}] Password reset for {userType}: {resetPasswordDto.Email}");
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Change password for authenticated user
        /// STORED PROCEDURE: SP_ChangePassword + SP_GetUserPasswordHash
        /// </summary>
        public async Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto, string userEmail)
        {
            if (changePasswordDto.NewPassword != changePasswordDto.ConfirmPassword)
            {
                throw new ArgumentException("New passwords do not match");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // ============================================================================
                // STORED PROCEDURE: SP_GetUserPasswordHash
                // Purpose: Get current password hash for verification
                // ============================================================================
                using (var command = new SqlCommand("SP_GetUserPasswordHash", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", userEmail);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            throw new KeyNotFoundException("User not found");
                        }

                        var currentPasswordHash = reader["PasswordHash"].ToString() ?? "";

                        // Verify current password
                        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, currentPasswordHash))
                        {
                            throw new UnauthorizedAccessException("Current password is incorrect");
                        }
                    }
                }

                // ============================================================================
                // STORED PROCEDURE: SP_ChangePassword
                // Purpose: Update password in appropriate table
                // ============================================================================
                using (var command = new SqlCommand("SP_ChangePassword", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", userEmail);
                    command.Parameters.AddWithValue("@NewPasswordHash",
                        BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var updatedCount = reader.GetInt32(reader.GetOrdinal("UpdatedCount"));
                            var userType = reader["UserType"].ToString() ?? "Unknown";

                            if (updatedCount > 0)
                            {
                                Console.WriteLine($"[{DateTime.UtcNow}] Password changed for {userType}: {userEmail}");
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        // ========== HELPER METHODS ==========

        private async Task<bool> EmailExistsAsync(string email)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SP_CheckEmailExists", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", email);

                    var result = await command.ExecuteScalarAsync();
                    return result != null && (bool)result;
                }
            }
        }
    }
}