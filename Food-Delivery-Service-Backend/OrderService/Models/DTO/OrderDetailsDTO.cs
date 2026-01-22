namespace OrderService.Models.DTO
{
    public class OrderDetailsDTO
    {
        public int orderId { get; set; }
        public decimal subtotal { get; set; }
        public byte status { get; set; }
        public bool cancellation_requested { get; set; }
        public List<OrderItemDetailsDTO> items { get; set; } = new List<OrderItemDetailsDTO>();
    }
}