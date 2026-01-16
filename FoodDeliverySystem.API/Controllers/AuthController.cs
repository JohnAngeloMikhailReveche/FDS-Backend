// ============================================================================
// FoodDeliverySystem.API/Controllers/AuthController.cs
// ============================================================================
// STORED PROCEDURES USED:
// 1-8: Original auth procedures
// 9. SP_ResetPassword       - Resets user password (Forgot Password)
// 10. SP_ChangePassword     - Changes password for authenticated user
// 11. SP_GetUserPasswordHash - Gets password hash for verification
// 12. SP_ToggleUserStatus   - Toggle user Active/Inactive status
// 13. SP_UpdateUserProfile  - Update user profile information
// ============================================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using FoodDeliverySystem.Application.DTOs;
using FoodDeliverySystem.Application.Interfaces;
using FoodDeliverySystem.Domain.Enums;

namespace FoodDeliverySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly string _connectionString;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        // ========== PUBLIC ENDPOINTS ==========

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                return Ok(new
                {
                    message = "Login successful",
                    user = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("register/customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterCustomerAsync(registerDto);
                return Ok(new
                {
                    message = "Customer registration successful",
                    user = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Reset password - Public endpoint (Forgot Password flow)
        /// STORED PROCEDURE: SP_ResetPassword
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                // ============================================================================
                // STORED PROCEDURE: SP_ResetPassword
                // Called by: AuthService.ResetPasswordAsync()
                // Purpose: Reset user password without authentication
                // Parameters: @Email, @NewPasswordHash
                // Returns: UpdatedCount (INT), UserType (NVARCHAR)
                // ============================================================================
                var result = await _authService.ResetPasswordAsync(resetPasswordDto);

                if (result)
                {
                    return Ok(new
                    {
                        message = "Password reset successful",
                        email = resetPasswordDto.Email
                    });
                }

                return BadRequest(new { message = "Failed to reset password" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // ========== AUTHENTICATED ENDPOINTS ==========

        /// <summary>
        /// Change password - Authenticated users only
        /// STORED PROCEDURES: SP_GetUserPasswordHash + SP_ChangePassword
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userEmail = GetCurrentUserEmail();

                // ============================================================================
                // STORED PROCEDURES USED:
                // 1. SP_GetUserPasswordHash - Verify current password
                //    Parameters: @Email
                //    Returns: PasswordHash, UserType
                // 
                // 2. SP_ChangePassword - Update password
                //    Parameters: @Email, @NewPasswordHash
                //    Returns: UpdatedCount (INT), UserType (NVARCHAR)
                // ============================================================================
                var result = await _authService.ChangePasswordAsync(changePasswordDto, userEmail);

                if (result)
                {
                    return Ok(new
                    {
                        message = "Password changed successfully",
                        email = userEmail
                    });
                }

                return BadRequest(new { message = "Failed to change password" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        // ========== ADMIN-ONLY ENDPOINTS ==========

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("create/admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto adminDto)
        {
            try
            {
                var creatorRole = GetCurrentUserRole();
                var creatorEmail = GetCurrentUserEmail();

                var result = await _authService.CreateAdminAsync(adminDto, creatorRole, creatorEmail);
                return Ok(new
                {
                    message = "Admin account created successfully",
                    admin = result,
                    createdBy = creatorEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPost("create/rider")]
        public async Task<IActionResult> CreateRider([FromBody] CreateRiderDto riderDto)
        {
            try
            {
                var creatorRole = GetCurrentUserRole();
                var creatorEmail = GetCurrentUserEmail();

                var result = await _authService.CreateRiderAsync(riderDto, creatorRole, creatorEmail);
                return Ok(new
                {
                    message = "Rider account created successfully",
                    rider = result,
                    createdBy = creatorEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto deleteDto)
        {
            try
            {
                var deleterRole = GetCurrentUserRole();
                var deleterEmail = GetCurrentUserEmail();

                var result = await _authService.DeleteAccountAsync(deleteDto.Email, deleterRole, deleterEmail);

                return Ok(new
                {
                    message = "Account deleted successfully",
                    deletedEmail = deleteDto.Email,
                    deletedBy = deleterEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var requesterRole = GetCurrentUserRole();
                var users = await _authService.GetAllUsersAsync(requesterRole);

                return Ok(new
                {
                    message = "Users retrieved successfully",
                    totalUsers = users.Count,
                    users = users
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Toggle user status (Active/Inactive) - Admin/SuperAdmin only
        /// STORED PROCEDURE: SP_ToggleUserStatus
        /// </summary>
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpPatch("toggle-status")]
        public async Task<IActionResult> ToggleUserStatus([FromBody] ToggleStatusDto toggleDto)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(toggleDto?.Email))
                {
                    return BadRequest(new { message = "Email is required" });
                }

                var requesterRole = GetCurrentUserRole();
                var requesterEmail = GetCurrentUserEmail();

                // Log for debugging
                Console.WriteLine($"========================================");
                Console.WriteLine($"Toggle Status Request:");
                Console.WriteLine($"  Target Email: {toggleDto.Email}");
                Console.WriteLine($"  Requester Email: {requesterEmail}");
                Console.WriteLine($"  Requester Role: {requesterRole} ({(int)requesterRole})");
                Console.WriteLine($"========================================");

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    Console.WriteLine("✅ Database connection opened");

                    // ============================================================================
                    // STORED PROCEDURE: SP_ToggleUserStatus
                    // Purpose: Toggle user's IsActive status with authorization checks
                    // Parameters: @Email, @RequesterEmail, @RequesterRole
                    // Returns: NewIsActive, UserRole, Email, FullName
                    // ============================================================================
                    using (var command = new SqlCommand("SP_ToggleUserStatus", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Email", toggleDto.Email);
                        command.Parameters.AddWithValue("@RequesterEmail", requesterEmail);
                        command.Parameters.AddWithValue("@RequesterRole", (int)requesterRole);

                        Console.WriteLine($"Executing SP with params:");
                        Console.WriteLine($"  @Email: {toggleDto.Email}");
                        Console.WriteLine($"  @RequesterEmail: {requesterEmail}");
                        Console.WriteLine($"  @RequesterRole: {(int)requesterRole}");

                        try
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (!await reader.ReadAsync())
                                {
                                    Console.WriteLine("❌ No data returned from stored procedure");
                                    return NotFound(new { message = "User not found or no data returned" });
                                }

                                // Read results
                                var newIsActive = reader.GetBoolean(reader.GetOrdinal("NewIsActive"));
                                var userRole = ((UserRole)reader.GetInt32(reader.GetOrdinal("UserRole"))).ToString();
                                var email = reader.GetString(reader.GetOrdinal("Email"));
                                var fullName = reader.GetString(reader.GetOrdinal("FullName"));

                                Console.WriteLine($"✅ Toggle successful:");
                                Console.WriteLine($"  Email: {email}");
                                Console.WriteLine($"  FullName: {fullName}");
                                Console.WriteLine($"  NewIsActive: {newIsActive}");
                                Console.WriteLine($"  Role: {userRole}");

                                return Ok(new
                                {
                                    message = $"User status updated to {(newIsActive ? "Active" : "Inactive")}",
                                    email = email,
                                    fullName = fullName,
                                    isActive = newIsActive,
                                    role = userRole,
                                    updatedBy = requesterEmail,
                                    timestamp = DateTime.UtcNow
                                });
                            }
                        }
                        catch (SqlException sqlEx)
                        {
                            Console.WriteLine($"❌ SQL Exception:");
                            Console.WriteLine($"  Number: {sqlEx.Number}");
                            Console.WriteLine($"  Message: {sqlEx.Message}");
                            Console.WriteLine($"  State: {sqlEx.State}");
                            Console.WriteLine($"  Source: {sqlEx.Source}");
                            Console.WriteLine($"  Procedure: {sqlEx.Procedure}");

                            // Handle RAISERROR from stored procedure
                            if (sqlEx.Number == 50000)
                            {
                                return BadRequest(new { message = sqlEx.Message });
                            }

                            // Other SQL errors
                            return StatusCode(500, new
                            {
                                message = "Database error occurred",
                                error = sqlEx.Message,
                                sqlErrorNumber = sqlEx.Number
                            });
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"❌ Unauthorized: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception in ToggleUserStatus:");
                Console.WriteLine($"  Type: {ex.GetType().Name}");
                Console.WriteLine($"  Message: {ex.Message}");
                Console.WriteLine($"  StackTrace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    message = "An error occurred while toggling user status",
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        // ========== USER INFO ENDPOINTS ==========

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userEmail = GetCurrentUserEmail();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SP_GetUserProfile", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Email", userEmail);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                            {
                                return NotFound(new { message = "User profile not found" });
                            }

                            var profile = new
                            {
                                id = Guid.Parse(reader["Id"].ToString() ?? Guid.Empty.ToString()),
                                fullName = reader["FullName"].ToString() ?? "",
                                email = reader["Email"].ToString() ?? "",
                                role = ((UserRole)reader.GetInt32("Role")).ToString(),
                                createdAt = reader.GetDateTime("CreatedAt"),
                                isActive = reader.GetBoolean("IsActive"),
                                contactNumber = reader["ContactNumber"] == DBNull.Value ? null : reader["ContactNumber"].ToString(),
                                motorcycleModel = reader["MotorcycleModel"] == DBNull.Value ? null : reader["MotorcycleModel"].ToString(),
                                plateNumber = reader["PlateNumber"] == DBNull.Value ? null : reader["PlateNumber"].ToString(),
                                phoneNumber = reader["PhoneNumber"] == DBNull.Value ? null : reader["PhoneNumber"].ToString(),
                                address = reader["Address"] == DBNull.Value ? null : reader["Address"].ToString()
                            };

                            return Ok(profile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Update user profile - Authenticated users only
        /// STORED PROCEDURE: SP_UpdateUserProfile
        /// </summary>
        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateDto)
        {
            try
            {
                var userEmail = GetCurrentUserEmail();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // ============================================================================
                    // STORED PROCEDURE: SP_UpdateUserProfile
                    // Purpose: Update user profile information
                    // Parameters: @Email, @FullName, @PhoneNumber, @Address
                    // Updates: Customer/Admin/Rider tables based on user type
                    // ============================================================================
                    using (var command = new SqlCommand("SP_UpdateUserProfile", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Email", userEmail);
                        command.Parameters.AddWithValue("@FullName", updateDto.FullName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@PhoneNumber", updateDto.PhoneNumber ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Address", updateDto.Address ?? (object)DBNull.Value);

                        try
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                        catch (SqlException sqlEx)
                        {
                            // Log the actual SQL error
                            Console.WriteLine($"SQL Error: {sqlEx.Message}");

                            if (sqlEx.Message.Contains("User not found"))
                            {
                                return NotFound(new { message = "User not found" });
                            }

                            return StatusCode(500, new
                            {
                                message = "Database error occurred",
                                error = sqlEx.Message,
                                sqlError = sqlEx.Number
                            });
                        }
                    }
                }

                return Ok(new
                {
                    message = "Profile updated successfully",
                    email = userEmail,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    message = "An error occurred",
                    error = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new
            {
                message = "Logout successful",
                note = "Please discard your JWT token on the client side"
            });
        }

        // ========== HELPER METHODS ==========

        private UserRole GetCurrentUserRole()
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (roleClaim == null)
                throw new UnauthorizedAccessException("No role claim found");

            return Enum.Parse<UserRole>(roleClaim.Value);
        }

        private string GetCurrentUserEmail()
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            return emailClaim?.Value ?? "unknown";
        }
    }

    // ========== DTOs ==========

    public class ToggleStatusDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}