using Microsoft.EntityFrameworkCore;
using KapeBara.MenuService.Models;

namespace KapeBara.MenuService.Data;

public class MenuDbContext : DbContext
{
    public MenuDbContext(DbContextOptions<MenuDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Variant> Variants => Set<Variant>();
    public DbSet<MenuItemVariant> MenuItemVariants => Set<MenuItemVariant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => c.Name).IsUnique();
        });

        // Seed fixed categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Classics" },
            new Category { Id = 2, Name = "Latte" },
            new Category { Id = 3, Name = "Frappe" },
            new Category { Id = 4, Name = "Refreshers" },
            new Category { Id = 5, Name = "Cupcakes" },
            new Category { Id = 6, Name = "Desserts" },
            new Category { Id = 7, Name = "Bowls" }
        );

        // MenuItem configuration
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasOne(m => m.Category)
                  .WithMany(c => c.MenuItems)
                  .HasForeignKey(m => m.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(m => m.Name);
        });

        // Variant configuration
        modelBuilder.Entity<Variant>(entity =>
        {
            entity.HasIndex(v => v.Name).IsUnique();
        });

        // MenuItemVariant configuration
        modelBuilder.Entity<MenuItemVariant>(entity =>
        {
            entity.HasOne(miv => miv.MenuItem)
                  .WithMany(m => m.MenuItemVariants)
                  .HasForeignKey(miv => miv.MenuItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(miv => miv.Variant)
                  .WithMany(v => v.MenuItemVariants)
                  .HasForeignKey(miv => miv.VariantId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Ensure unique combination of MenuItem and Variant
            entity.HasIndex(miv => new { miv.MenuItemId, miv.VariantId }).IsUnique();
        });
    }
}
