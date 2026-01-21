using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using OrderService.Models.DTO;
using OrderService.Services;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly OrderDbContext _db;
        private readonly IHttpClientFactory _httpFactory;
        private readonly CartService _cartService;


        public CartController(OrderDbContext db, IHttpClientFactory httpFactory)
        {
            _db = db;
            _httpFactory = httpFactory;
            _cartService = new CartService(db, httpFactory);
        }

        // Add Item to Cart Endpoint
        [HttpPost("item/add")]
        public async Task<IActionResult> AddToCart(int menuItemID, int variantId, int userID, string specialInstructions)
        {
            try
            {
                CartDTO? cart = await _cartService.AddItem(
                    menuItemID,
                    variantId,
                    userID,
                    specialInstructions
                    );

                if (cart == null)
                    return BadRequest(new { message = "Cart not found." });

                return Ok(cart);

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // View the Cart
        [HttpGet("get-cart/{userId}")]
        public async Task<IActionResult> ViewCart(int userId)
        {
            var cart = await _cartService.ViewCart(userId);

            if (cart == null)
            {
                return NotFound(new { message = "Cart not found." });
            }

            return Ok(cart);
        }

        // Remove Cart Item
        [HttpDelete("remove-item/{cartItemID}")]
        public async Task<IActionResult> RemoveItemFromCart(
            [FromRoute] int cartItemID,
            [FromQuery] int userID,
            [FromQuery] int quantityToRemove
            )
        {
            var cart = await _cartService.RemoveItem(userID, cartItemID, quantityToRemove);

            if (cart == null)
                return NotFound(new { message = "Item or cart is not found." });

            return Ok(cart);
        }


        // Increase Cart Item Quantity
        [HttpPatch("update/{cartItemID}/increase")]
        public async Task<IActionResult> IncreaseItemQuantity(
                [FromRoute] int cartItemID,
                [FromQuery] int userID,
                [FromQuery] int count = 1
            )
        {
            var cart = await _cartService.IncreaseItem(userID, cartItemID, count);

            if (cart == null)
            {
                return NotFound(new { message = "Item or the cart is not found." });
            }

            return Ok(cart);
        }


    }
}