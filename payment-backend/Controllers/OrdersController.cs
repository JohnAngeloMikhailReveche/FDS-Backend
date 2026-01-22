using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PaymentService2.Models;
using PaymentService2.Services;


namespace PaymentService2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPayMongoService _payMongoService;

    private string ResolveUserId(string? userId)
    {
        // Admins may pass an explicit userId; regular users use their token subject
        if (!string.IsNullOrWhiteSpace(userId) && User.IsInRole("Admin")) return userId;
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(claim)) return claim;
        return "user_001";
    }

    public OrdersController(IOrderService orderService, IPayMongoService payMongoService)
    {
        _orderService = orderService;
        _payMongoService = payMongoService;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromQuery] string? userId = null)
    {
        try
        {
            var resolvedUserId = ResolveUserId(userId);
            var order = await _orderService.CreateOrderAsync(resolvedUserId, request);
            return Ok(new OrderResponse
            {
                Success = true,
                Data = order,
                Message = "Order created successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new OrderResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(string orderId)
    {
        var order = await _orderService.GetOrderAsync(orderId);
        if (order == null)
        {
            return NotFound(new OrderResponse
            {
                Success = false,
                Message = "Order not found"
            });
        }

        return Ok(new OrderResponse
        {
            Success = true,
            Data = order
        });
    }

    /// <summary>
    /// Get all orders for a user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<OrderListResponse>> GetOrders(
        [FromQuery] string? userId = null,
        [FromQuery] int limit = 10)
    {
        var resolvedUserId = ResolveUserId(userId);
        var orders = await _orderService.GetOrdersAsync(resolvedUserId, limit);
        return Ok(new OrderListResponse
        {
            Success = true,
            Data = orders,
            Total = orders.Count
        });
    }

    /// <summary>
    /// Complete a pending order (after payment)
    /// </summary>
    [HttpPost("{orderId}/complete")]
    public async Task<ActionResult<OrderResponse>> CompleteOrder(string orderId)
    {
        try
        {
            var order = await _orderService.CompleteOrderAsync(orderId);
            return Ok(new OrderResponse
            {
                Success = true,
                Data = order,
                Message = "Order completed successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new OrderResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Pay an existing pending order (wallet payment completes immediately).
    /// </summary>
    [HttpPost("{orderId}/pay")]
    public async Task<ActionResult<OrderResponse>> PayOrder(
        string orderId,
        [FromBody] PayOrderRequest request,
        [FromQuery] string? userId = null)
    {
        try
        {
            var resolvedUserId = ResolveUserId(userId);
            var order = await _orderService.PayOrderAsync(resolvedUserId, orderId, request);
            return Ok(new OrderResponse
            {
                Success = true,
                Data = order,
                Message = "Order paid successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new OrderResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Proactively verify payment status with PayMongo
    /// </summary>
    [HttpPost("{orderId}/verify")]
    public async Task<ActionResult<OrderResponse>> VerifyOrder(string orderId)
    {
        try
        {
            var order = await _orderService.GetOrderAsync(orderId);
            if (order == null) return NotFound(new OrderResponse { Success = false, Message = "Order not found" });

            // Only check if pending and has session ID
            if (order.Status == "pending" && !string.IsNullOrEmpty(order.CheckoutSessionId))
            {
                // Fetch latest status from PayMongo
                var session = await _payMongoService.GetCheckoutSessionAsync(order.CheckoutSessionId);
                
                // Check if any payment object implies 'paid'
                var payments = session?.Attributes?.Payments;
                if (payments != null && payments.Any(p => p.Attributes?.Status == "paid"))
                {
                    // Payment confirmed -> Complete order
                    order = await _orderService.CompleteOrderAsync(orderId); // This logic needs to be robust (e.g. inventory)
                    return Ok(new OrderResponse 
                    { 
                        Success = true, 
                        Data = order, 
                        Message = "Payment verified and order completed" 
                    });
                }
            }

            return Ok(new OrderResponse 
            { 
                Success = true, 
                Data = order, 
                Message = "Order status unchanged" 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new OrderResponse
            {
                Success = false,
                Message = $"Verification failed: {ex.Message}"
            });
        }
    }
}
