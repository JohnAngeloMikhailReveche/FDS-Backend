// FoodDeliverySystem.Application/Services/AuthService.cs
using FoodDeliverySystem.Application.DTOs;
using FoodDeliverySystem.Application.Interfaces;
using FoodDeliverySystem.Common.Helpers;
using FoodDeliverySystem.Domain.Entities;
using FoodDeliverySystem.Domain.Enums;
using FoodDeliverySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(ApplicationDbContext context, JwtSettings jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings;
        }

        // ========== AUTHENTICATION METHODS ==========

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Check all user tables
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Email == loginDto.Email && a.IsActive);

            var rider = await _context.Riders
                .FirstOrDefaultAsync(r => r.Email == loginDto.Email && r.IsActive);

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == loginDto.Email && c.IsActive);

            User? user = admin as User ?? rider as User ?? customer as User;

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            var token = JwtHelper.GenerateToken(user.Email, user.Role, user.Id.ToString(), _jwtSettings);

            return new AuthResponseDto
            {
                Token = token,
                Email = user.Email,
                Role = user.Role.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
            };
        }

        public async Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerDto registerDto)
        {
            // Customers can register themselves without admin
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                throw new ArgumentException("Passwords do not match");
            }

            if (await EmailExistsAsync(registerDto.Email))
            {
                throw new ArgumentException("Email already exists");
            }

            var customer = new Customer
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = UserRole.Customer,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[{DateTime.UtcNow}] Customer self-registered: {customer.Email}");

            var token = JwtHelper.GenerateToken(customer.Email, customer.Role, customer.Id.ToString(), _jwtSettings);

            return new AuthResponseDto
            {
                Token = token,
                Email = customer.Email,
                Role = customer.Role.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
            };
        }

        // ========== ADMIN-ONLY ACCOUNT CREATION ==========

        public async Task<AuthResponseDto> CreateAdminAsync(CreateAdminDto adminDto, UserRole creatorRole, string creatorEmail)
        {
            // Only SuperAdmin can create Admin accounts
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

            var admin = new Admin
            {
                FullName = adminDto.FullName,
                Email = adminDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminDto.Password),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[{DateTime.UtcNow}] Admin account created by {creatorEmail}");
            Console.WriteLine($"  New Admin: {admin.Email} ({admin.FullName})");

            var token = JwtHelper.GenerateToken(admin.Email, admin.Role, admin.Id.ToString(), _jwtSettings);

            return new AuthResponseDto
            {
                Token = token,
                Email = admin.Email,
                Role = admin.Role.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
            };
        }

        public async Task<AuthResponseDto> CreateRiderAsync(CreateRiderDto riderDto, UserRole creatorRole, string creatorEmail)
        {
            // Only SuperAdmin or Admin can create Rider accounts
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

            var rider = new Rider
            {
                FullName = riderDto.FullName,
                Email = riderDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(riderDto.Password),
                Role = UserRole.Rider,
                ContactNumber = riderDto.ContactNumber,
                MotorcycleModel = riderDto.MotorcycleModel,
                PlateNumber = riderDto.PlateNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Riders.Add(rider);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[{DateTime.UtcNow}] Rider account created by {creatorEmail}");
            Console.WriteLine($"  New Rider: {rider.Email} ({rider.FullName}) - {rider.PlateNumber}");

            var token = JwtHelper.GenerateToken(rider.Email, rider.Role, rider.Id.ToString(), _jwtSettings);

            return new AuthResponseDto
            {
                Token = token,
                Email = rider.Email,
                Role = rider.Role.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
            };
        }

        // ========== ADMIN-ONLY ACCOUNT MANAGEMENT ==========

        public async Task<bool> DeleteAccountAsync(string email, UserRole deleterRole, string deleterEmail)
        {
            // Only SuperAdmin or Admin can delete accounts
            if (deleterRole != UserRole.SuperAdmin && deleterRole != UserRole.Admin)
            {
                throw new UnauthorizedAccessException(
                    $"Only Super Admin or Admin can delete accounts. Your role: {deleterRole}");
            }

            // Cannot delete yourself
            if (email == deleterEmail)
            {
                throw new InvalidOperationException("You cannot delete your own account");
            }

            // Check and delete from appropriate table
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
            if (admin != null)
            {
                // Additional check: Only SuperAdmin can delete other Admins
                if (admin.Role == UserRole.Admin && deleterRole != UserRole.SuperAdmin)
                {
                    throw new UnauthorizedAccessException("Only Super Admin can delete Admin accounts");
                }

                _context.Admins.Remove(admin);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[{DateTime.UtcNow}] Admin account deleted by {deleterEmail}");
                Console.WriteLine($"  Deleted Admin: {email}");
                return true;
            }

            var rider = await _context.Riders.FirstOrDefaultAsync(r => r.Email == email);
            if (rider != null)
            {
                _context.Riders.Remove(rider);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[{DateTime.UtcNow}] Rider account deleted by {deleterEmail}");
                Console.WriteLine($"  Deleted Rider: {email}");
                return true;
            }

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                Console.WriteLine($"[{DateTime.UtcNow}] Customer account deleted by {deleterEmail}");
                Console.WriteLine($"  Deleted Customer: {email}");
                return true;
            }

            throw new KeyNotFoundException($"Account with email '{email}' not found");
        }

        public async Task<List<UserInfoDto>> GetAllUsersAsync(UserRole requesterRole)
        {
            // Only SuperAdmin or Admin can view all users
            if (requesterRole != UserRole.SuperAdmin && requesterRole != UserRole.Admin)
            {
                throw new UnauthorizedAccessException(
                    $"Only Super Admin or Admin can view all users. Your role: {requesterRole}");
            }

            var allUsers = new List<UserInfoDto>();

            // Get all Admins
            var admins = await _context.Admins.ToListAsync();
            allUsers.AddRange(admins.Select(a => new UserInfoDto
            {
                Id = a.Id,
                FullName = a.FullName,
                Email = a.Email,
                Role = a.Role.ToString(),
                CreatedAt = a.CreatedAt,
                IsActive = a.IsActive
            }));

            // Get all Riders
            var riders = await _context.Riders.ToListAsync();
            allUsers.AddRange(riders.Select(r => new UserInfoDto
            {
                Id = r.Id,
                FullName = r.FullName,
                Email = r.Email,
                Role = r.Role.ToString(),
                CreatedAt = r.CreatedAt,
                IsActive = r.IsActive,
                ContactNumber = r.ContactNumber,
                MotorcycleModel = r.MotorcycleModel,
                PlateNumber = r.PlateNumber
            }));

            // Get all Customers
            var customers = await _context.Customers.ToListAsync();
            allUsers.AddRange(customers.Select(c => new UserInfoDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                Role = c.Role.ToString(),
                CreatedAt = c.CreatedAt,
                IsActive = c.IsActive,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address
            }));

            return allUsers.OrderBy(u => u.Role).ThenBy(u => u.Email).ToList();
        }

        private async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Admins.AnyAsync(a => a.Email == email) ||
                   await _context.Riders.AnyAsync(r => r.Email == email) ||
                   await _context.Customers.AnyAsync(c => c.Email == email);
        }
    }
}