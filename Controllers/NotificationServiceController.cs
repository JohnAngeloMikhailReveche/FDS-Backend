using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Models;
using NotificationService.Integration.Email;

namespace NotificationService.Controllers
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
            return await _context.Notifications.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Notification>> GetNotification(int id) 
        {
            var notification = await _context.Notifications.FindAsync(id); 

            if (notification == null) return NotFound();

            return notification;
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

            return CreatedAtAction("GetNotification", new { id = notification.Id }, notification);
        }
    }
}