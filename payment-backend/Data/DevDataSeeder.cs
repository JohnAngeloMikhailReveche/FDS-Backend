using Microsoft.EntityFrameworkCore;
using PaymentService2.Models;

namespace PaymentService.Data;

public static class DevDataSeeder
{
    public static async Task SeedAsync(PaymentDbContext db)
    {
        // Vouchers
        if (!await db.Vouchers.AnyAsync())
        {
            db.Vouchers.AddRange(
                new Voucher
                {
                    Id = "vch_save20",
                    Code = "SAVE20",
                    Description = "20% off (min ₱200)",
                    DiscountType = "percentage",
                    DiscountValue = 20,
                    MinimumPurchase = 200,
                    MaxDiscount = 100,
                    ValidFrom = DateTime.UtcNow.AddDays(-1),
                    ValidUntil = DateTime.UtcNow.AddDays(30),
                    IsActive = true,
                    UsageLimit = 999,
                    UsageCount = 0
                },
                new Voucher
                {
                    Id = "vch_freedel",
                    Code = "FREEDEL",
                    Description = "₱60 off (min ₱300)",
                    DiscountType = "fixed",
                    DiscountValue = 60,
                    MinimumPurchase = 300,
                    MaxDiscount = null,
                    ValidFrom = DateTime.UtcNow.AddDays(-1),
                    ValidUntil = DateTime.UtcNow.AddDays(14),
                    IsActive = true,
                    UsageLimit = 999,
                    UsageCount = 0
                },
                new Voucher
                {
                    Id = "vch_coffee50",
                    Code = "COFFEE50",
                    Description = "₱50 off (min ₱150)",
                    DiscountType = "fixed",
                    DiscountValue = 50,
                    MinimumPurchase = 150,
                    MaxDiscount = null,
                    ValidFrom = DateTime.UtcNow.AddDays(-1),
                    ValidUntil = DateTime.UtcNow.AddDays(45),
                    IsActive = true,
                    UsageLimit = 999,
                    UsageCount = 0
                }
            );
        }

        var userId = "user_001";

        // Mock order data (DB-backed default):
        // Since there is currently no external Order Service, the unified app uses DB-backed /api/orders as a
        // mock Order Service. This dev seeding ensures Checkout has multiple pending orders to choose from.
        //
        // Rewire later:
        // - When a real Order Service exists, set OrderService:Enabled=true + OrderService:BaseUrl and start using
        //   /api/order-integration/* (or refactor IOrderService to proxy externally).
        // - At that point, you can remove/trim this dev seeding if you no longer want local mock orders.

        const int desiredPendingOrders = 6; // 1 existing + 5 more to populate the dropdown

        var pendingOrders = await db.Orders
            .Where(o => o.UserId == userId && o.Status == "pending")
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        // Repair any existing pending order that has no items (prevents Checkout looking broken).
        foreach (var po in pendingOrders)
        {
            if (po.Items.Count != 0) continue;
            po.Items = new List<OrderItem>
            {
                new OrderItem { Name = "Iced Coffee", Quantity = 1, Price = 100m },
                new OrderItem { Name = "Delivery Fee", Quantity = 1, Price = 50m }
            };
            var computed = po.Items.Sum(i => i.Price * i.Quantity);
            po.Amount = Math.Max(0m, computed - po.DiscountAmount);
        }

        // Add more pending orders (ORD-103, ORD-104, ...), skipping IDs that already exist.
        // IMPORTANT: only seed these ORD-10x orders once. Do not "replenish" them after the user pays/completes orders,
        // otherwise the pending count will jump back up on every backend restart.
        var hasOrdSeedOrders = await db.Orders.AnyAsync(o => o.UserId == userId && o.Id.StartsWith("ORD-"));
        if (!hasOrdSeedOrders)
        {
            var paymentMethods = new[] { "gcash", "maya", "card", "wallet", "grab_pay" };
            var nextNumber = 103;
            while (pendingOrders.Count < desiredPendingOrders)
            {
                var id = $"ORD-{nextNumber}";
                nextNumber++;

                if (await db.Orders.AnyAsync(o => o.Id == id))
                {
                    continue;
                }

                var idx = pendingOrders.Count;
                var items = new List<OrderItem>
                {
                    new OrderItem { Name = idx % 2 == 0 ? "Iced Coffee" : "Iced Latte", Quantity = 1, Price = idx % 2 == 0 ? 100m : 150m },
                    new OrderItem { Name = idx % 3 == 0 ? "Chocolate Chip Cookie" : "Croissant", Quantity = 1, Price = idx % 3 == 0 ? 95m : 85m },
                    new OrderItem { Name = "Delivery Fee", Quantity = 1, Price = 50m }
                };

                var computed = items.Sum(i => i.Price * i.Quantity);
                var newOrder = new Order
                {
                    Id = id,
                    UserId = userId,
                    Branch = "Main Branch",
                    Items = items,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-5 * (idx + 1)),
                    VoucherCode = null,
                    DiscountAmount = 0m,
                    PaymentMethod = paymentMethods[idx % paymentMethods.Length],
                    Amount = computed
                };

                db.Orders.Add(newOrder);
                pendingOrders.Add(newOrder);
            }
        }

        // Wallet
        var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
        {
            wallet = new Wallet
            {
                UserId = userId,
                Balance = 1250m,
                Coins = 120,
                LastUpdated = DateTime.UtcNow
            };
            db.Wallets.Add(wallet);
        }

        // Transactions
        if (!await db.Transactions.AnyAsync(t => t.UserId == userId))
        {
            db.Transactions.AddRange(
                new Transaction
                {
                    Id = "txn_topup_001",
                    UserId = userId,
                    Type = "topup",
                    Amount = 1000m,
                    Description = "Top-up via GCash",
                    ReferenceId = "topup_001",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Transaction
                {
                    Id = "txn_coins_001",
                    UserId = userId,
                    Type = "coins",
                    Amount = 120m,
                    Description = "Coins earned (seed)",
                    ReferenceId = "topup_001",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Transaction
                {
                    Id = "txn_order_001",
                    UserId = userId,
                    Type = "order",
                    Amount = -295m,
                    Description = "Order: Iced Latte + Cookie",
                    ReferenceId = "ord_000001",
                    CreatedAt = DateTime.UtcNow.AddHours(-6)
                }
            );
        }

        // Orders
        if (!await db.Orders.AnyAsync(o => o.UserId == userId))
        {
            db.Orders.AddRange(
                new Order
                {
                    Id = "ord_000001",
                    UserId = userId,
                    Amount = 295m,
                    Branch = "Main Branch",
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Name = "Iced Latte", Quantity = 1, Price = 150m },
                        new OrderItem { Name = "Chocolate Chip Cookie", Quantity = 1, Price = 95m },
                        new OrderItem { Name = "Delivery Fee", Quantity = 1, Price = 50m }
                    },
                    Status = "completed",
                    CreatedAt = DateTime.UtcNow.AddHours(-6),
                    VoucherCode = null,
                    DiscountAmount = 0m,
                    PaymentMethod = "wallet"
                },
                new Order
                {
                    Id = "ord_000002",
                    UserId = userId,
                    Amount = 450m,
                    Branch = "Main Branch",
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Name = "Cappuccino", Quantity = 2, Price = 180m },
                        new OrderItem { Name = "Delivery Fee", Quantity = 1, Price = 90m }
                    },
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    VoucherCode = null,
                    DiscountAmount = 0m,
                    PaymentMethod = "gcash"
                }
            );
        }

        // Refunds
        if (!await db.Refunds.AnyAsync(r => r.UserId == userId))
        {
            db.Refunds.Add(new RefundRequest
            {
                UserId = userId,
                OrderId = "ord_000002",
                CustomerName = "Demo Customer",
                CustomerEmail = "demo@kapebara.local",
                CustomerPhone = "09170000000",
                Amount = 180m,
                Reason = "Item quality issue",
                Category = "Quality Issue",
                Status = RefundStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            });
        }

        // Ensure there is a pending refund request for ORD-106 so we can test admin approval + wallet credit.
        // This is dev-only scaffolding to make manual testing easier.
        var ord106 = await db.Orders.FirstOrDefaultAsync(o => o.Id == "ORD-106");
        if (ord106 != null)
        {
            var hasRefundForOrd106 = await db.Refunds.AnyAsync(r => r.OrderId == ord106.Id);
            if (!hasRefundForOrd106)
            {
                db.Refunds.Add(new RefundRequest
                {
                    UserId = ord106.UserId,
                    OrderId = ord106.Id,
                    CustomerName = "Demo Customer",
                    CustomerEmail = "demo@kapebara.local",
                    CustomerPhone = "09170000000",
                    Amount = ord106.Amount,
                    Reason = "Requested refund (test)",
                    Category = "Other",
                    Status = RefundStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
