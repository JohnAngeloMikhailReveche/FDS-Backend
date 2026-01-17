using FoodDeliverySystem.Domain.Entities;
using FoodDeliverySystem.Domain.Enums;
using FoodDeliverySystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliverySystem.Infrastructure.Seeders
{
    public static class DatabaseSeeder
    {
        public static async Task SeedSuperAdminAsync(ApplicationDbContext context)
        {
            // Check if super admin already exists
            var superAdminExists = await context.Admins
                .AnyAsync(a => a.Email == "superadmin@fooddelivery.com");

            if (!superAdminExists)
            {
                var superAdmin = new Admin
                {
                    FullName = "Super Admin",
                    Email = "superadmin@fooddelivery.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = UserRole.SuperAdmin,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                context.Admins.Add(superAdmin);
                await context.SaveChangesAsync();
            }
        }
    }
}