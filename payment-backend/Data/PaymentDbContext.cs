using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaymentService2.Models;

namespace PaymentService.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    // Register ALL your tables here
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<RefundRequest> Refunds { get; set; } = null!; // Use RefundRequest class for the 'Refunds' table
    public DbSet<TopUp> TopUps { get; set; } = null!;
    public DbSet<Voucher> Vouchers { get; set; } = null!;
    public DbSet<Wallet> Wallets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ensure all DateTime values coming from EF are treated as UTC.
        // Without this, SQLite round-trips often lose DateTimeKind (becoming Unspecified),
        // and the API will serialize timestamps without a 'Z' suffix, causing inconsistent
        // timezone interpretation in the browser.
        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        );

        var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue
                ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc))
                : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
        );

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(utcConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableUtcConverter);
                }
            }
        }

        // This fixes the "List<OrderItem>" error
        // It tells EF that OrderItems belong strictly to an Order
        modelBuilder.Entity<Order>().OwnsMany(o => o.Items);

        // Optional: Configure decimal precision (money) to avoid warnings
        modelBuilder.Entity<Order>().Property(o => o.Amount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Transaction>().Property(t => t.Amount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Wallet>().Property(w => w.Balance).HasColumnType("decimal(18,2)");
    }
}