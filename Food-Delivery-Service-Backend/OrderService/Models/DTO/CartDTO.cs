namespace OrderService.Models.DTO
{
    public class CartDTO
    {
        public int cart_id { get; set; }
        public int users_id { get; set; }
        public decimal subtotal { get; set; }
        public DateTime updated_at { get; set; }
        public List<CartItemDTO>? cartItems { get; set; }
    }
}
