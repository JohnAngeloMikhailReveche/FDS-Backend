using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Models;
using NotificationService.Services.Email;

namespace NotificationService.Controllers;

[Route("api/notifications")]
[ApiController]
[Produces("application/json")] // Defines that this API always returns JSON
public class NotificationServiceController : ControllerBase
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationServiceController : ControllerBase
    {
        private readonly NotificationContext _context;
        private readonly GmailService _gmailService;

        public NotificationServiceController(NotificationContext context, GmailService gmailService)
        {
            _context = context;
            _gmailService = gmailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
            return NotFound(new ErrorResponse
            {
                Error = "NO_NOTIFICATIONS",
                Message = "No notifications found"
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id) 
        {
            var notification = await _context.Notifications.FindAsync(id); 

            if (notification == null) return NotFound();

    // POST: /api/notifications/notify
    [HttpPost("notify")]
    [ProducesResponseType(typeof(NotificationResponse), 201)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    public async Task<ActionResult<NotificationResponse>> PostNotification([FromBody] CreateNotificationRequest request)
    {
        if (string.IsNullOrEmpty(request.Type) || string.IsNullOrEmpty(request.Message) || string.IsNullOrEmpty(request.Title))
        {
            return BadRequest(new ErrorResponse
            { 
                Error = "INVALID_REQUEST", 
                Message = "Missing required field 'type' or 'message'" 
            });
        }

        [HttpPost("notify")]
        public async Task<ActionResult<Notification>> PostNotification(Notification notification)
        {
            if (string.IsNullOrEmpty(notification.Type) || string.IsNullOrEmpty(notification.Message))
            {
                return BadRequest("Type and Message are required.");
            }

            if (notification.Type.ToLower() == "email")
            {
                if (string.IsNullOrEmpty(notification.EmailAddress))
                {
                    return BadRequest("Email address is required.");
                }

                try
                {
                    await _gmailService.SendEmailAsync(notification.EmailAddress, notification.Title, notification.Message);
                }
                catch (Exception ex)
                {

                    return StatusCode(500, $"Failed to send email: {ex.Message}");
                }
            }

            
            notification.CreatedAt = DateTime.UtcNow;
            notification.Status = "unread";

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return Ok(new DeleteResponse
            {
                UserId = userId,
                Message = "All notifications successfully deleted."
            });
        }
    }
}