namespace PaymentService.Models;

public class Transaction
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = "user_001";
    public string Type { get; set; } = string.Empty; // topup, order, refund
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; } // TopUp ID or Order ID
    public DateTime CreatedAt { get; set; }
}

public class TransactionListResponse
{
    public bool Success { get; set; }
    public List<Transaction> Data { get; set; } = new();
    public int Total { get; set; }
}
