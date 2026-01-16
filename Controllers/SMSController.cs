using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using System.Security.Claims;
using NotificationService.Interfaces;
using NuGet.Protocol.Core.Types;

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


    /// Send Telegram message notification to user
    [HttpPost("send-sms")]
    public async Task<IActionResult> SendSMSNotification(CreateNotificationDTO notificationDTO)
    {
        var userId = GetUserId();

        var notificationId = await _smsService.SendSMSAsync(userId, notificationDTO);
        return Ok(new
        {
            message = "Message sent through SMS.",
            notificationId
        });          
    }
}
