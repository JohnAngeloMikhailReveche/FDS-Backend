namespace PaymentService.Models;

public class TopUp
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = "user_001";
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = "pending"; // pending, completed, failed
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? PaymentLinkId { get; set; } // PayMongo link ID
    public string? PaymentLinkUrl { get; set; } // PayMongo checkout URL
}

public class TopUpRequest
{
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // gcash, maya, card
}

public class TopUpResponse
{
    public bool Success { get; set; }
    public TopUp? Data { get; set; }
    public string? Message { get; set; }
    public string? CheckoutUrl { get; set; } // For PayMongo redirect
}

public class TopUpListResponse
{
    public bool Success { get; set; }
    public List<TopUp> Data { get; set; } = new();
    public int Total { get; set; }
}
