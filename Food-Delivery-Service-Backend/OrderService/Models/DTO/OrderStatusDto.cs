namespace OrderService.Models.DTO
{
    public class OrderStatusDto
    {
        public int OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string? UserRole { get; set; }
    }

    public enum OrderStatus
    {
        Placed = 1,
        Preparing = 2,
        ReadyForPickup = 3,
        InTransit = 4,
        Delivered = 5,
        Failed = 6,
        Cancelled = 7
    }
}