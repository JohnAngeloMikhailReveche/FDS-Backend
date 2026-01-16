using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using NotificationService.Interfaces;

namespace NotificationService.Controllers;

// [Authorize]
[ApiController]
[Route("api/notifications/")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;   
    }

    private string GetUserId()
    {
        // return User.FindFirstValue(ClaimTypes.NameIdentifier)
        //     ?? User.FindFirstValue("sub")
        //     ?? throw new UnauthorizedAccessException("User ID not found in token.");

        return "123";
    }


    /// Send email notification to user
    [HttpPost("send-email")]
    public async Task<IActionResult> SendEmailNotification(CreateNotificationDTO notificationDto)
    {
        var userId = GetUserId();

        try
        {
            var notificationId = await _emailService.SendEmailNotificationAsync(userId, notificationDto);

            return Ok(new
            {
                message = "Email sent.",
                notificationId
            }); 
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Failed to send email.",
                error = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }
}
