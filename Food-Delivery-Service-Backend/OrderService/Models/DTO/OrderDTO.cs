namespace OrderService.Models.DTO
{
    public class OrderDTO
    {
        public int orders_id { get; set; }
        public string users_id { get; set; }
        public byte status { get; set; }
        public decimal subtotal { get; set; }
        public decimal total_cost { get; set; }
        public int item_count { get; set; }
        public DateTime placed_at { get; set; }
        public DateTime? fulfilled_at { get; set; }
        public string? payment_method { get; set; }
        public bool cancellation_requested { get; set; }
        public string? cancellation_reason { get; set; }
        public int? payment_id { get; set; }
        public byte? refund_status { get; set; }
        public List<OrderItemDTO>? orderItems { get; set; }
    }
}