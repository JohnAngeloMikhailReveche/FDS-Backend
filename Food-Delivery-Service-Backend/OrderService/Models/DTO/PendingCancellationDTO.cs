namespace OrderService.Models.DTO
{
    public class PendingCancellationDTO
    {
        public int orders_id { get; set; }
        public int users_id { get; set; }
        public string? cancellation_reason { get; set; }
        public int item_count { get; set; }
        public string Status { get; set; }
        public decimal total_cost { get; set; }
        public DateTime placed_at { get; set; }
    }
}