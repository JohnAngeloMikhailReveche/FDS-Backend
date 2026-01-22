using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models.DTO;
using OrderService.Services;
using System.Security.Claims;

namespace OrderService.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/[controller]")]
	public class OrdersController : ControllerBase
	{
		private readonly OrderService.Services.OrderService _orderService;

		public OrdersController(OrderService.Services.OrderService orderService)
		{
			_orderService = orderService;
		}

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? throw new UnauthorizedAccessException("User ID not found in token.");
        }

        // ============================================
        // CUSTOMER ENDPOINTS
        // ============================================

        /// <summary>
        /// Request order cancellation
        /// </summary>
        [HttpPatch("request-cancellation")]
		public async Task<IActionResult> RequestCancellation([FromBody] CancellationRequestDto request)
		{
			if (request == null || string.IsNullOrWhiteSpace(request.Reason))
				return BadRequest(new { message = "A reason is required to cancel." });

			try
			{
				var result = await _orderService.RequestCancellationAsync(request.OrderId, request.Reason);

				if (result.Contains("submitted") || result.Contains("Wait"))
					return Ok(new { message = result });

				return BadRequest(new { message = result });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}

		/// <summary>
		/// Place a new order from cart
		/// </summary>
		[HttpPost("place/order/{userId}")]
		public async Task<IActionResult> PlaceOrder()
		{

			var userId = GetUserId();

			try
			{
				var (orderId, message) = await _orderService.PlaceOrderAsync(userId);

				if (orderId == 0)
					return BadRequest(new { message });

				return Ok(new { orderId, message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}

		/// <summary>
		/// Get order details with items
		/// </summary>
		[HttpGet("{orderId}/items")]
		public async Task<IActionResult> GetOrderDetails(int orderId)
		{
			try
			{
				var order = await _orderService.GetOrderDetailsAsync(orderId);

				if (order == null)
					return NotFound(new { message = "Order not found." });

				return Ok(order);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}

		/// <summary>
		/// Update order status
		/// </summary>
		[HttpPatch("status")]
		public async Task<IActionResult> UpdateStatus([FromBody] OrderStatusDto request)
		{
			if (request == null)
				return BadRequest(new { message = "Request body is empty." });

			try
			{
				string role = request.UserRole?.ToLower() ?? "customer";
				var result = await _orderService.UpdateStatusAsync(
					request.OrderId,
					(byte)request.NewStatus,
					role
				);

				if (result.Contains("Success") || result.Contains("success"))
					return Ok(new { message = result });

				return BadRequest(new { message = result });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}

		/// <summary>
		/// Get order history for a user
		/// </summary>
		[HttpGet("history/{userId}")]
		public async Task<IActionResult> GetOrderHistory(
			[FromQuery] string filter = "all",
			[FromQuery] string sortOrder = "newest")
		{

            var userId = GetUserId();

            try
			{
				var orders = await _orderService.GetOrderHistoryAsync(userId, filter, sortOrder);

				if (!orders.Any())
					return Ok(new { message = $"No {filter} orders found." });

				return Ok(orders);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}

		// ============================================
		// ADMIN ENDPOINTS
		// ============================================

		/// <summary>
		/// Get all order history (admin view)
		/// </summary>
		[HttpGet("admin/history")]
		public async Task<IActionResult> GetAdminOrderHistory(
			[FromQuery] string filter = "all",
			[FromQuery] string sortOrder = "newest")
		{
			try
			{
				// Pass null for userId to get all orders
				var orders = await _orderService.GetOrderHistoryAsync(null, filter, sortOrder);

				if (!orders.Any())
					return Ok(new { message = $"No {filter} orders found." });

				return Ok(orders);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}

		/// <summary>
		/// Get pending cancellation requests
		/// </summary>
		[HttpGet("admin/pending-cancellations")]
		public async Task<IActionResult> GetPendingCancellations()
		{
			try
			{
				var pending = await _orderService.GetPendingCancellationsAsync();

				if (!pending.Any())
					return Ok(new { message = "No pending cancellations." });

				return Ok(pending);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}

		/// <summary>
		/// Review a cancellation request (approve or decline)
		/// </summary>
		[HttpPost("admin/review-cancellation")]
		public async Task<IActionResult> ReviewCancellation([FromBody] AdminCancellationReviewDto review)
		{
			if (review == null)
				return BadRequest(new { message = "Request body is empty." });

			try
			{
				var result = await _orderService.ReviewCancellationAsync(review.OrderId, review.Approve);

				return Ok(new { message = result });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = ex.Message });
			}
		}
	}
}