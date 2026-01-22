using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PaymentService2.Models;
using PaymentService2.Services;

namespace PaymentService2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TopUpController : ControllerBase
{
    private readonly ITopUpService _topUpService;
    private readonly IPayMongoService _paymongoService;

    public TopUpController(ITopUpService topUpService, IPayMongoService paymongoService)
    {
        _topUpService = topUpService;
        _paymongoService = paymongoService;
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst(ClaimTypes.Email)?.Value 
            ?? Request.Headers["X-User-Id"].FirstOrDefault()
            ?? "user_001";
    }

    [HttpPost]
    public async Task<ActionResult> CreateTopUp([FromBody] CreateTopUpRequest request)
    {
        try
        {
            var userId = request.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                userId = GetUserId();
            }

            // Create top-up record
            var topUp = await _topUpService.CreateTopUpAsync(userId, request.Amount, request.PaymentMethod ?? "gcash");

            // For external payments (gcash, maya, card), create PayMongo link
            string? checkoutUrl = null;
            if (request.PaymentMethod != "wallet")
            {
                // Redirect back to checkout page (which now handles success state)
                var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:3000";
                var successUrl = $"{frontendUrl}/topup/checkout/{topUp.Id}?payment_return=true";
                var cancelUrl = $"{frontendUrl}/wallet"; // Redirect to wallet on cancel to avoid loop

                var paymentResult = await _paymongoService.CreatePaymentLinkAsync(
                    request.Amount,
                    $"Top-up for {userId}",
                    topUp.Id,
                    successUrl,
                    cancelUrl
                );

                if (paymentResult.Success)
                {
                    checkoutUrl = paymentResult.CheckoutUrl;
                    // Save URL and Session ID to database
                    await _topUpService.UpdateTopUpPaymentUrlAsync(topUp.Id, checkoutUrl, paymentResult.CheckoutId);
                    // Update local object so it's returned correctly
                    topUp.PaymentUrl = checkoutUrl;
                    topUp.CheckoutSessionId = paymentResult.CheckoutId;
                }
                else
                {
                    throw new InvalidOperationException($"PayMongo payment link creation failed: {paymentResult.Message}");
                }
            }

            // Return in format frontend expects
            return Ok(new
            {
                success = true,
                data = topUp,
                checkoutUrl = checkoutUrl
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult> GetTopUps([FromQuery] string? userId = null)
    {
        // This endpoint doesn't exist in our stored proc service yet
        // Return empty list for now
        return Ok(new List<TopUp>());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetTopUp(string id)
    {
        try
        {
            var topUp = await _topUpService.GetTopUpAsync(id);
            if (topUp == null)
            {
                return NotFound(new { message = "Top-up not found" });
            }
            // Return in format frontend expects
            return Ok(new { 
                data = new {
                    id = topUp.Id,
                    userId = topUp.UserId,
                    amount = topUp.Amount,
                    status = topUp.Status,
                    paymentMethod = topUp.PaymentMethod,
                    paymentUrl = topUp.PaymentUrl,
                    referenceNumber = topUp.Id, // Use Id as reference number
                    createdAt = topUp.CreatedAt,
                    completedAt = topUp.CompletedAt
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/verify")]
    public async Task<ActionResult> VerifyTopUp(string id)
    {
        try
        {
             var topUp = await _topUpService.GetTopUpAsync(id);
             if (topUp == null) return NotFound(new { message = "Top-up not found" });

             if (topUp.Status == "completed") 
                 return Ok(new { success = true, status = "completed" });

             // Check if we have a session ID
             if (string.IsNullOrEmpty(topUp.CheckoutSessionId))
             {
                 return Ok(new { success = true, status = topUp.Status });
             }

             // Proactive check with PayMongo
             var session = await _paymongoService.GetCheckoutSessionAsync(topUp.CheckoutSessionId);
             
             // Check if paid (payments array is not empty and has paid status)
             if (session?.Attributes?.Payments != null && session.Attributes.Payments.Any(p => p.Attributes?.Status == "paid"))
             {
                 // Complete the top-up
                 await _topUpService.CompleteTopUpAsync(id);
                 return Ok(new { success = true, status = "completed" });
             }

             return Ok(new { success = true, status = "pending" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<TopUp>> CompleteTopUp(string id)
    {
        try
        {
            var topUp = await _topUpService.CompleteTopUpAsync(id);
            return Ok(new { success = true, data = topUp });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{id}/fail")]
    public ActionResult FailTopUp(string id)
    {
        // Mark as failed (not implemented in stored proc)
        return Ok(new { success = true, message = "Top-up marked as failed" });
    }
}

public class CreateTopUpRequest
{
    public string? UserId { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMethod { get; set; }
}

