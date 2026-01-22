namespace OrderService.Models.DTO
{
    public class OrderHistoryDTO
    {
        public int orders_id { get; set; }
        public string users_id { get; set; }
        public decimal total_cost { get; set; }
        public DateTime placed_at { get; set; }
        public DateTime? fulfilled_at { get; set; }
        public int item_count { get; set; }
        public byte StatusValue { get; set; }
        public string StatusName { get; set; }
        public string? payment_method { get; set; }
        public bool cancellation_requested { get; set; }
        public string? cancellation_reason { get; set; }
    }
}