namespace PaymentService2.Models;

public class OrderResponse
{
    public bool Success { get; set; }
    public Order? Data { get; set; }
    public string? Message { get; set; }
}

public class OrderListResponse
{
    public bool Success { get; set; }
    public List<Order> Data { get; set; } = new();
    public int Total { get; set; }
}

public class CreateOrderRequest
{
    public List<OrderItem> Items { get; set; } = new();
    public string Branch { get; set; } = string.Empty;
    public string? VoucherCode { get; set; }
    public string PaymentMethod { get; set; } = "wallet"; // wallet, gcash, maya, card
    public int CoinsToUse { get; set; }
}

public class PayOrderRequest
{
    public string PaymentMethod { get; set; } = "wallet"; // wallet, gcash, maya, card
    public string? VoucherCode { get; set; }
    public int CoinsToUse { get; set; }
}

public class CompleteOrderRequest
{
    public string OrderId { get; set; } = string.Empty;
}
