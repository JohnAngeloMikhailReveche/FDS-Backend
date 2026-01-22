using Microsoft.AspNetCore.Mvc;
using PaymentService.Integrations;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/order-integration")]
public class OrderIntegrationController : ControllerBase
{
    // NOTE (Rewire hook):
    // These endpoints are an opt-in proxy to an external Order Service.
    // They are disabled by default (OrderService:Enabled=false) so the app can run standalone using the DB-backed
    // /api/orders endpoints as a mock Order Service.
    private readonly IOrderServiceClient _orderService;

    public OrderIntegrationController(IOrderServiceClient orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("orders/{orderId}")]
    public async Task<IActionResult> GetOrder(string orderId, CancellationToken ct)
    {
        if (!_orderService.IsConfigured)
        {
            return StatusCode(503, new { message = "Order Service integration is disabled or not configured. Set OrderService:Enabled=true and OrderService:BaseUrl." });
        }

        try
        {
            var order = await _orderService.GetOrderAsync(orderId, ct);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = ex.Message });
        }
    }

    [HttpGet("users/{userId}/pending-count")]
    public async Task<IActionResult> GetPendingCount(string userId, CancellationToken ct)
    {
        if (!_orderService.IsConfigured)
        {
            return StatusCode(503, new { message = "Order Service integration is disabled or not configured. Set OrderService:Enabled=true and OrderService:BaseUrl." });
        }

        try
        {
            var pending = await _orderService.GetPendingCountAsync(userId, ct);
            return Ok(new { pendingCount = pending });
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = ex.Message });
        }
    }
}
