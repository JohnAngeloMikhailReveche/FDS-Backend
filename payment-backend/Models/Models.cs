namespace PaymentService2.Models;

public class Wallet
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public int Coins { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class Transaction
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? ReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Order
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending";
    public string? PaymentMethod { get; set; }
    public string PaymentStatus { get; set; } = "pending";
    public string? VoucherCode { get; set; }
    public decimal VoucherDiscount { get; set; }
    public int CoinsUsed { get; set; }
    public decimal CoinsDiscount { get; set; }
    public decimal FinalAmount { get; set; }
    public string? Branch { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? PaymentUrl { get; set; }
    public string? PaymentLinkId { get; set; }
    public string? CheckoutSessionId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class TopUp
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending";
    public string? PaymentMethod { get; set; }
    public string? PaymentUrl { get; set; }
    public string? CheckoutSessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class Voucher
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "percentage";
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; }
    public decimal? MaxDiscount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class RefundRequest
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? Category { get; set; }
    public string Status { get; set; } = "pending";
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? PhotoPath { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? ReviewedBy { get; set; }
    public bool WalletCredited { get; set; }
    public string? VoucherCode { get; set; }
    public decimal VoucherDiscount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

// DTOs for API responses
public class WalletResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Wallet? Data { get; set; }
}

public class TransactionListResponse
{
    public bool Success { get; set; }
    public List<Transaction> Data { get; set; } = new();
    public int Total { get; set; }
}

public class VoucherApplyResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public decimal Discount { get; set; }
}

public class ApplyVoucherRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
}
