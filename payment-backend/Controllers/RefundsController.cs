using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PaymentService2.Models;
using PaymentService2.Services;

namespace PaymentService2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RefundsController : ControllerBase
{
    private readonly IRefundService _refundService;

    public RefundsController(IRefundService refundService)
    {
        _refundService = refundService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RefundRequest>>> GetAllRefunds([FromQuery] string? status = null)
    {
        var refunds = await _refundService.GetRefundsAsync(null, status);
        return Ok(refunds);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<List<RefundRequest>>> GetUserRefunds(string userId)
    {
        var refunds = await _refundService.GetRefundsAsync(userId);
        return Ok(refunds);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RefundRequest>> GetRefund(string id)
    {
        var refund = await _refundService.GetRefundAsync(id);
        if (refund == null) return NotFound();
        return Ok(refund);
    }

    [HttpPost]
    public async Task<ActionResult<RefundRequest>> CreateRefund([FromBody] CreateRefundRequest request)
    {
        try
        {
            var refund = await _refundService.CreateRefundAsync(
                request.UserId,
                request.OrderId,
                request.Amount,
                request.Reason,
                request.Category,
                request.CustomerName,
                request.CustomerEmail,
                request.CustomerPhone
            );
            return Ok(refund);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/review")]
    public async Task<ActionResult<RefundRequest>> ReviewRefund(string id, [FromBody] ReviewRefundRequest request)
    {
        try
        {
            var refund = await _refundService.ReviewRefundAsync(
                id,
                request.Action,
                request.AdminNotes,
                request.RejectionReason,
                request.ReviewedBy
            );
            return Ok(refund);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/process")]
    public async Task<ActionResult<RefundRequest>> ProcessRefund(string id)
    {
        try
        {
            var refund = await _refundService.ProcessRefundToWalletAsync(id);
            return Ok(refund);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpPost("with-photo")]
    public async Task<ActionResult<RefundRequest>> CreateRefundWithPhoto([FromForm] CreateRefundWithPhotoRequest request)
    {
        try
        {
            string? photoPath = null;
            
            // Handle photo upload
            if (request.Photo != null && request.Photo.Length > 0)
            {
                // Create uploads directory if it doesn't exist
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "refunds");
                Directory.CreateDirectory(uploadsFolder);
                
                // Generate unique filename
                var fileExtension = Path.GetExtension(request.Photo.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Photo.CopyToAsync(stream);
                }
                
                // Store relative path for database
                photoPath = $"/uploads/refunds/{uniqueFileName}";
            }
            
            var refund = await _refundService.CreateRefundAsync(
                request.UserId,
                request.OrderId,
                request.Amount,
                request.Reason,
                request.Category,
                request.CustomerName,
                request.CustomerEmail,
                request.CustomerPhone,
                photoPath
            );
            return Ok(refund);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CreateRefundWithPhotoRequest
{
    public string UserId { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? Category { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public IFormFile? Photo { get; set; }
}

public class CreateRefundRequest
{
    public string UserId { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? Category { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
}

public class ReviewRefundRequest
{
    public string Action { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? ReviewedBy { get; set; }
}
