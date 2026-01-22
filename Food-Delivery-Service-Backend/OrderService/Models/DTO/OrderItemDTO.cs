namespace OrderService.Models.DTO
{
	public class OrderItemDTO
	{
		public int order_item_id { get; set; }
		public int orders_id { get; set; }
		public string item_name { get; set; }
		public string? item_description { get; set; }
		public string? variant_name { get; set; }
		public decimal variant_price { get; set; }
		public int quantity { get; set; }
		public string? special_instructions { get; set; }
		public decimal line_subtotal { get; set; }
		public string? img_url { get; set; }
	}
}