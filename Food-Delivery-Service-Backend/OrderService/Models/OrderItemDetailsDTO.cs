namespace OrderService.Models.DTO
{
    public class OrderItemDetailsDTO
    {
        public string? imageUrl { get; set; }
        public string name { get; set; }
        public int quantity { get; set; }
        public string size { get; set; }
        public decimal total { get; set; }
        public string? specialInstructions { get; set; }
    }
}