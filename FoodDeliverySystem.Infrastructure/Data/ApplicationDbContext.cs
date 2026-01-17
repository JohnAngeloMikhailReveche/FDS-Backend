using Microsoft.EntityFrameworkCore;
using FoodDeliverySystem.Domain.Entities;

namespace FoodDeliverySystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Rider> Riders { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names
            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<Rider>().ToTable("Riders");
            modelBuilder.Entity<Customer>().ToTable("Customers");

            // Configure indexes
            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<Rider>()
                .HasIndex(r => r.Email)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();
        }
    }
}