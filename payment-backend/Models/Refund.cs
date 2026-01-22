using System.Text.Json.Serialization;

namespace PaymentService.Models;

public class RefundRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;  // Links to wallet
    public string OrderId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Wrong Order", "Quality Issue", "Late Delivery", "Other"
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RefundStatus Status { get; set; } = RefundStatus.Pending;
    
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? PhotoPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? WalletTransactionId { get; set; }  // Transaction ID when wallet is credited
    public bool WalletCredited { get; set; } = false;
}

public enum RefundStatus
{
    Pending,
    UnderReview,
    Approved,
    Rejected,
    Completed
}

public class RefundRequestDto
{
    public string UserId { get; set; } = string.Empty;  // Required for wallet credit
    public string OrderId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }
}

public class ReviewRefundDto
{
    public string Action { get; set; } = string.Empty; // "approve" or "reject"
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string ReviewedBy { get; set; } = "Admin";
}

public class ContactCustomerDto
{
    public string RefundId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ContactMethod { get; set; } = "email"; // "email" or "sms"
}

public class RefundStats
{
    public int TotalRequests { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public decimal TotalRefundedAmount { get; set; }
}
