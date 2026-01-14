using Microsoft.EntityFrameworkCore;

namespace NotificationService.Models;

public class NotificationServiceContext : DbContext
{
    public NotificationServiceContext(DbContextOptions<NotificationServiceContext> options)
        : base(options){}

    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>()
        .HasOne(n => n.User)
        .WithMany(u => u.Notifications)
        .HasForeignKey(n => n.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}