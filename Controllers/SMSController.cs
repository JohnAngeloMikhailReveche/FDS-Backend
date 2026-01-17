using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using System.Security.Claims;
using NotificationService.Interfaces;

namespace NotificationService.Controllers;

// [Authorize]
[Route("api/notifications/")]
[ApiController]
public class SMSController : ControllerBase
{
    private readonly ISMSService _smsService;
    public SMSController(ISMSService smsService)
    {
        _smsService = smsService;
    }

    private string GetUserId()
    {
        // return User.FindFirstValue(ClaimTypes.NameIdentifier)
        //     ?? User.FindFirstValue("sub")
        //     ?? throw new UnauthorizedAccessException();

        return "123";
    }


    /// Send SMS notification to user
    [HttpPost("send-sms")]
    public async Task<IActionResult> SendSMSNotification(CreateNotificationDTO notificationDTO)
    {
        var userId = GetUserId();

        try
        {
            var notificationId = await _smsService.SendSMSAsync(userId, notificationDTO);
            return Ok(new
            {
                message = "Message sent through SMS.",
                notificationId
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new 
            { 
                message = "Failed to send SMS. SMS provider error.",
                error = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Failed to send SMS.",
                error = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }
}
