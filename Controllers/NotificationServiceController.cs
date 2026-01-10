using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Repositories;
using NotificationService.DTOs;
using NotificationService.Extensions;

namespace NotificationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationServiceController : ControllerBase
    {
        private readonly INotificationRepository _repository;

        public NotificationServiceController(INotificationRepository repository)
        {
            _repository = repository;
        }

        // GET: api/NotificationService
        // [Authorize]
        [HttpGet("inbox")]
        public async Task<ActionResult<IEnumerable<NotificationResponseDTO>>> GetNotifications()
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notifications = await _repository.GetAllNotificationsAsync(userId);
            var response = notifications.Select(n => n.ToResponseDTO()).ToList();
            
            return Ok(response);
        }

        // GET: api/NotificationService/5
        // [Authorize]
        [HttpGet("{id}/inbox")]
        public async Task<ActionResult<NotificationResponseDTO>> GetNotification(int id)
        {            
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();    

            var notification = await _repository.GetNotificationAsync(userId, id);

            if (notification == null)
                return NotFound();

            return Ok(notification.ToResponseDTO());
        }


        // POST: api/SendEmailNotification
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // [Authorize]
        [HttpPost("send-via-email")]
        public async Task<ActionResult<Notification>> SendEmailNotification(CreateNotificationDTO notificationDto)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrEmpty(notificationDto.EmailAddress))
                return BadRequest(new { message = "EmailAddress is required for email notifications." });

            var notification = new Notification
            {
                TargetUserId = userId,
                Title = notificationDto.Title,
                Message = notificationDto.Message,
                Type = "Email",
                EmailAddress = notificationDto.EmailAddress,
                PhoneNumber = notificationDto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                Status = "Sent via email",
                IsRead = false
            };

            var notificationId = await _repository.AddNotificationAsync(notification);
            notification.Id = notificationId;
            
            return CreatedAtAction(nameof(GetNotification), new { id = notificationId }, notification);          
        }


        //  POST: api/SendInAppNotification
        // [Authorize]
        [HttpPost("send-via-app")]
        public async Task<ActionResult<Notification>> SendInAppNotification(CreateNotificationDTO notificationDto)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notification = new Notification
            {
                TargetUserId = userId,
                Title = notificationDto.Title,
                Message = notificationDto.Message,
                Type = "In-App",
                EmailAddress = notificationDto.EmailAddress,
                PhoneNumber = notificationDto.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                Status = "unread",
                IsRead = false
            };

            var notificationId = await _repository.AddNotificationAsync(notification);
            notification.Id = notificationId;

            return CreatedAtAction(nameof(GetNotification), new { id = notificationId }, notification);
        }

        // PUT: api/NotificationService
        // [Authorize]
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _repository.MarkAllAsReadAsync(userId);

            if (!success)
                return NotFound(new { message = "No notifications to mark as read." });

            return Ok(new { message = "All notifications marked as read." });
        }

        // PUT: api/NotificationService/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // [Authorize]
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var notification = await _repository.GetNotificationAsync(userId, id);

            if (notification == null)
                return NotFound(new { message = "Notification not found." });

            var success = await _repository.MarkAsReadAsync(userId, id);
            
            if (!success)
                return StatusCode(500, new { message = "Failed to mark notification as read." });

            return Ok(new { message = "Notification marked as read.", id = id });
        }
        
        // DELETE: api/NotificationService
        // [Authorize]
        [HttpDelete("remove-all")]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _repository.DeleteAllNotificationsAsync(userId);
            
            if (!success)
                return NotFound(new { message = "No notifications to be deleted."});

            return Ok(new { message = "All notifications deleted."});
        }
  
        // DELETE: api/NotificationService/5
        // [Authorize]
        [HttpDelete("{id}/remove")]
        public async Task<IActionResult> DeleteNotification(int id)
        {  
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var notification = await _repository.GetNotificationAsync(userId, id);

            if (notification == null)
            {
                return NotFound(new { message = "Notification not found."});
            }

            var success = await _repository.DeleteNotificationAsync(userId, id);
            
            if (!success)
                return StatusCode(500, new { message = "Failed to delete notification." });

            return Ok(new { message = "Notification deleted."});
        }
    }
}

