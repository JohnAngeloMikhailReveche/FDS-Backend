using Microsoft.EntityFrameworkCore;

namespace NotificationService.Models;

public class NotificationContext : DbContext
{
    public NotificationContext(DbContextOptions<NotificationContext> options)
        : base(options){}

    public DbSet<Notification> Notifications { get; set; } = null!;
}