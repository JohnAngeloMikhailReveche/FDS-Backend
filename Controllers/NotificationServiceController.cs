using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.Repositories;
using NotificationService.DTOs;
using NotificationService.Mapping;
using NotificationService.Helpers;
using Google.Apis.Gmail.v1;
using System.Security.Claims;

namespace NotificationService.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationServiceController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly GmailEmailService _gmail;

        public NotificationServiceController(
            IHttpClientFactory httpClientFactory,
            INotificationRepository notificationRepository, 
            IUserRepository userRepository, 
            GmailEmailService gmail)
        {
            _httpClientFactory = httpClientFactory;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _gmail = gmail; 
        }


        // GET: api/NotificationService
        // [Authorize]
        [HttpGet("messages")]
        public async Task<ActionResult<IEnumerable<NotificationResponseDTO>>> GetNotifications()
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var userExisting = await _userRepository.GetUserByIdAsync(userId);
            if (userExisting == null)
                return NotFound("User not found.");

            var notifications = await _notificationRepository.GetAllNotificationsAsync(userId);
            var response = notifications.Select(n => n.ToResponseDTO()).ToList();
            
            return Ok(response);
        }


        // GET: api/NotificationService/5
        // [Authorize]
        [HttpGet("{id}/message")]
        public async Task<ActionResult<NotificationResponseDTO>> GetNotification(int id)
        {            
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();    

            var userExisting = await _userRepository.GetUserByIdAsync(userId);
            if (userExisting == null)
                return NotFound("User not found.");
            
            var notification = await _notificationRepository.GetNotificationAsync(userId, id);

            if (notification == null)
                return NotFound();

            return Ok(notification.ToResponseDTO());
        }

        
        // POST: api/CreateUser
        // [Authorize]
        [HttpPost("user-create/send-message")]
        public async Task<ActionResult<Notification>> NotifyUserCreation(
            CreateUserDTO userDto, [FromServices] IHttpClientFactory httpClientFactory)
        {

            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "234";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = new User
            {
                Id = userId,
                Email = userDto.Email,
                PhoneNumber = userDto.PhoneNumber
            };

            await _userRepository.CreateUserAsync(user);

            // Create and send in-app notification
            var notificationDto = new CreateNotificationDTO
            {
                Subject = "Welcome",
                Body = $"User account created successfully",
            };

            var notificationResult = await SendInAppNotification(notificationDto);

            return notificationResult;
        }


        // POST: api/SendEmailNotification
        // [Authorize]
        [HttpPost("gmail/send-message")]
        public async Task<ActionResult<Notification>> SendEmailNotification(
            CreateNotificationDTO notificationDto)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (email, phoneNumber) = await _userRepository.GetUserContactAsync(userId);

            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { 
                    message = "User email not found in database.", 
                    email = email, phoneNumber = phoneNumber });

            email = email.Trim();
            
            if (!email.Contains("@"))
                return BadRequest(new { 
                    message = "Invalid email address in database.", 
                    email = email, emailLength = email.Length 
                    });

            var notification = new Notification
            {
                UserId = userId,
                Subject = notificationDto.Subject,
                Body = notificationDto.Body,
                Type = "Email",
                CreatedAt = DateTime.UtcNow,
                Status = "Sent via email",
                IsRead = false
            };

            var notificationId = await _notificationRepository.AddNotificationAsync(notification);
            
            try
            {
                await _gmail.SendEmailAsync(
                    email, 
                    notificationDto.Subject, 
                    notificationDto.Body
                    );
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    message = "Failed to send email.", 
                    error = ex.Message 
                    });
            }
            
            return Ok(new { message = "Email sent.", notificationId = notificationId });          
        }


        // POST: api/SendTelegramNotification
        // [Authorize]
        [HttpPost("telegram/send-message")]
        public async Task<ActionResult<Notification>> SendTelegramNotification(
            CreateNotificationDTO notificationDto)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            
            await _userRepository.GetOrCreateUserAsync(userId);

            var notification = new Notification
            {
                UserId = userId,
                Subject = notificationDto.Subject,
                Body = notificationDto.Body,
                Type = "Telegram",
                CreatedAt = DateTime.UtcNow,
                Status = "unread",
                IsRead = false
            };

            // Telegram code here
            // try
            // 
               // 
 
            // catch
            // {

            // }            }


            var notificationId = await _notificationRepository.AddNotificationAsync(notification);
            notification.Id = notificationId;

            return Ok(new { message = "Message sent.", notificationId = notificationId });          
        }



        //  POST: api/SendInAppNotification
        // [Authorize]
        [HttpPost("in-app/send-message")]
        public async Task<ActionResult<Notification>> SendInAppNotification(
            CreateNotificationDTO notificationDto)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Create or get the user with email and phone number if provided
            await _userRepository.GetOrCreateUserAsync(userId);

            var notification = new Notification
            {
                UserId = userId,
                Subject = notificationDto.Subject,
                Body = notificationDto.Body,
                Type = "In-App",
                CreatedAt = DateTime.UtcNow,
                Status = "unread",
                IsRead = false
            };

            var notificationId = await _notificationRepository.AddNotificationAsync(notification);
            notification.Id = notificationId;

            return CreatedAtAction(nameof(GetNotification), new { id = notificationId }, notification);
        }


        // PaymentService will use this
        // [Authorize]
        [HttpPost("inbox/save")]
        public async Task<ActionResult<Notification>> SaveMessage(CreateNotificationDTO notificationDtO)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Create or get the user with email and phone number if provided
            await _userRepository.GetOrCreateUserAsync(userId);
            
            var notification = new Notification
            {
                UserId = userId,
                Subject = notificationDtO.Subject,
                Body = notificationDtO.Body,
                Type = "Payment sent.",
                CreatedAt = DateTime.UtcNow,
                Status = "unread",
                IsRead = false
            };

            var notificationId = await _notificationRepository.AddNotificationAsync(notification);
            notification.Id = notificationId;

            return CreatedAtAction(nameof(GetNotification), new { id = notificationId }, notification);
        }


        // PUT: api/NotificationService
        // [Authorize]
        [HttpPut("inbox/mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await _notificationRepository.MarkAllAsReadAsync(userId);

            if (!success)
                return NotFound(new { message = "No notifications to mark as read." });

            return Ok(new { message = "All notifications marked as read." });
        }


        // PUT: api/NotificationService/5
        // [Authorize]
        [HttpPut("inbox/{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var notification = await _notificationRepository.GetNotificationAsync(userId, id);

            if (notification == null)
                return NotFound(new { message = "Notification not found." });

            var success = await _notificationRepository.MarkAsReadAsync(userId, id);
            
            if (!success)
                return StatusCode(500, new { message = "Failed to mark notification as read." });

            return Ok(new { message = "Notification marked as read.", id = id });
        }

        // PUT: api/users/{userId}/update
        // [Authorize]
        [HttpPut("users/{userId}/update")]
        public async Task<IActionResult> UpdateUser(CreateNotificationDTO updateDto)
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "User ID is required." });

            // Verify user exists
            var existingUser = await _userRepository.GetUserByIdAsync(userId);
            if (existingUser == null)
                return NotFound(new { message = "User not found." });

            var success = await _userRepository.UpdateUserAsync(userId); 

            if (!success)
                return StatusCode(500, new { message = "Failed to update user." });

            return Ok(new { message = "User updated successfully.", userId = userId });
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

            var success = await _notificationRepository.DeleteAllNotificationsAsync(userId);
            
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

            var notification = await _notificationRepository.GetNotificationAsync(userId, id);

            if (notification == null)
            {
                return NotFound(new { message = "Notification not found."});
            }

            var success = await _notificationRepository.DeleteNotificationAsync(userId, id);
            
            if (!success)
                return StatusCode(500, new { message = "Failed to delete notification." });

            return Ok(new { message = "Notification deleted."});
        }


        // DELETE: api/users/{userId}/remove
        // [Authorize]
        [HttpDelete("users/{userId}/remove")]
        public async Task<IActionResult> DeleteUser()
        {
            // var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var userId = "123";

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "User ID is required." });

            var existingUser = await _userRepository.GetUserByIdAsync(userId);
            if (existingUser == null)
                return NotFound(new { message = "User not found." });

            var success = await _userRepository.DeleteUserAsync(userId);

            if (!success)
                return StatusCode(500, new { message = "Failed to delete user." });

            return Ok(new { message = "User deleted successfully.", userId = userId });
        }
    }
}

