namespace OrderService.Models.DTO
{
    public class CartItemDTO
    {
        public int cart_item_id { get; set; }
        public string item_name { get; set; }
        public string item_description { get; set; }
        public string variant_name { get; set; }
        public decimal variant_price { get; set; }
        public int quantity { get; set; }
        public string specialInstructions { get; set; }
        public string img_url { get; set; }
    }
}
