// FoodDeliverySystem.API/Controllers/AuthController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FoodDeliverySystem.Application.DTOs;
using FoodDeliverySystem.Application.Interfaces;
using FoodDeliverySystem.Domain.Enums;
using FoodDeliverySystem.Infrastructure.Data;

namespace FoodDeliverySystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        // Fixed constructor with 2 parameters
        public AuthController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
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

        // ========== USER INFO ENDPOINTS ==========

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userEmail = GetCurrentUserEmail();
                var userRole = GetCurrentUserRole();

                // Find user in appropriate table
                if (userRole == UserRole.SuperAdmin || userRole == UserRole.Admin)
                {
                    var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == userEmail);
                    if (admin != null)
                    {
                        return Ok(new
                        {
                            id = admin.Id,
                            fullName = admin.FullName,
                            email = admin.Email,
                            role = admin.Role.ToString(),
                            createdAt = admin.CreatedAt,
                            isActive = admin.IsActive
                        });
                    }
                }
                else if (userRole == UserRole.Rider)
                {
                    var rider = await _context.Riders.FirstOrDefaultAsync(r => r.Email == userEmail);
                    if (rider != null)
                    {
                        return Ok(new
                        {
                            id = rider.Id,
                            fullName = rider.FullName,
                            email = rider.Email,
                            role = rider.Role.ToString(),
                            contactNumber = rider.ContactNumber,
                            motorcycleModel = rider.MotorcycleModel,
                            plateNumber = rider.PlateNumber,
                            createdAt = rider.CreatedAt,
                            isActive = rider.IsActive
                        });
                    }
                }
                else if (userRole == UserRole.Customer)
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
                    if (customer != null)
                    {
                        return Ok(new
                        {
                            id = customer.Id,
                            fullName = customer.FullName,
                            email = customer.Email,
                            role = customer.Role.ToString(),
                            phoneNumber = customer.PhoneNumber,
                            address = customer.Address,
                            createdAt = customer.CreatedAt,
                            isActive = customer.IsActive
                        });
                    }
                }

                return NotFound(new { message = "User profile not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
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
}