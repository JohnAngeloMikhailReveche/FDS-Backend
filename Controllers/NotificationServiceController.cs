using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Models;
using System.Security.Claims; // Added for future Auth logic

namespace NotificationService.Controllers;

[Route("api/notifications")]
[ApiController]
[Produces("application/json")] // Defines that this API always returns JSON
public class NotificationServiceController : ControllerBase
{
    private readonly NotificationContext _context;

    public NotificationServiceController(NotificationContext context)
    {
        _context = context;
    }

    // GET: api/notifications
    [HttpGet]
    [ProducesResponseType(typeof(NotificationListResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    public async Task<ActionResult<NotificationListResponse>> GetNotifications()
    {
        // [PRODUCTION]: Uncomment to retrieve the userId from the Auth token
        // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // if (string.IsNullOrEmpty(userId)) return Unauthorized(new ErrorResponse { Error = "UNAUTHORIZED", Message = "User not logged in" });

        // [TESTING]: Hardcoded user
        var userId = "user456";
        
        var notifications = await _context.Notifications
            .Where(n => n.TargetUserId == userId) // Filter by user
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        if (notifications == null || !notifications.Any())
        {
            return NotFound(new ErrorResponse
            {
                Error = "NO_NOTIFICATIONS",
                Message = "No notifications found for this user"
            });
        }

        var response = new NotificationListResponse
        {
            Notifications = notifications.Select(n => new NotificationResponse
            {
                Id = n.Id.ToString(),
                UserId = n.TargetUserId,
                Type = "account", 
                Title = n.Title,
                Message = n.Message,
                Status = n.IsRead ? "read" : "unread",
                CreatedAt = n.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                UpdatedAt = n.ReadAt?.ToString("yyyy-MM-ddTHH:mm:ssZ")
            })
        };

        return Ok(response);
    }

    // GET: api/notifications/{notifId}
    [HttpGet("{notifId}")]
    [ProducesResponseType(typeof(NotificationResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 403)]
    public async Task<ActionResult<NotificationResponse>> GetNotification([FromRoute] int notifId)
    {
        // [PRODUCTION]: Uncomment Auth
        // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        // [TESTING]: Hardcoded user
        var userId = "user456";

        var notification = await _context.Notifications.FindAsync(notifId);

        if (notification == null)
        {
            return NotFound(new ErrorResponse
            {
                Error = "NO_NOTIFICATIONS",
                Message = "No notifications found"
            });
        }

        // [PRODUCTION]: Uncomment ownership check
        // if (notification.TargetUserId != userId) 
        // {
        //     return StatusCode(403, new ErrorResponse { Error = "ACCESS_DENIED", Message = "You cannot access this notification" });
        // }

        var response = new NotificationResponse
        {
            Id = notification.Id.ToString(),
            UserId = notification.TargetUserId,
            Type = "account",
            Title = notification.Title,
            Message = notification.Message,
            Status = notification.IsRead ? "read" : "unread",
            CreatedAt = notification.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            UpdatedAt = notification.ReadAt?.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };

        return Ok(response);
    }

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

        // [PRODUCTION]: Auth & Permission
        // var targetUserId = request.UserId ?? User.FindFirst("sub")?.Value; 
        
        // [TESTING]: Hardcoded user
        var targetUserId = "user456"; 

        var notification = new Notification
        {
            Title = request.Title,
            Message = request.Message,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            TargetUserId = targetUserId 
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var response = new NotificationResponse
        {
            Id = notification.Id.ToString(), 
            UserId = targetUserId,
            Type = request.Type,
            Title = notification.Title,
            Message = notification.Message,
            Status = "unread",
            CreatedAt = notification.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ExtraData = request.ExtraData
        };

        return CreatedAtAction(nameof(GetNotification), new { notifId = notification.Id }, response);
    }

    // PATCH: /api/notifications/{notifId}
    [HttpPatch("{notifId}")]
    [ProducesResponseType(typeof(NotificationResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 400)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<ActionResult<NotificationResponse>> UpdateNotificationStatus([FromRoute] int notifId, [FromBody] UpdateStatusRequest request)
    {
        // [PRODUCTION]: Auth
        // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // if (userId == null) return Unauthorized(new ErrorResponse { Error = "UNAUTHORIZED", Message = "User is not authenticated" });

        // [TESTING]: Hardcoded user
        var userId = "user456";

        if (request == null || string.IsNullOrWhiteSpace(request.Status) || request.Status.ToLower() != "read")
        {
            return BadRequest(new ErrorResponse
            { 
                Error = "INVALID_REQUEST", 
                Message = "Invalid status. Only 'read' is supported." 
            });
        }

        try 
        {
            var notification = await _context.Notifications.FindAsync(notifId);

            if (notification == null) 
            {
                return NotFound(new ErrorResponse
                { 
                    Error = "NO_NOTIFICATIONS", 
                    Message = "No notifications found" 
                });
            }

            // [PRODUCTION]: Ownership Logic
            // if (notification.TargetUserId != userId)
            // {
            //    return StatusCode(403, new ErrorResponse { Error = "ACCESS_DENIED", Message = "Access denied" });
            // }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();

            var response = new NotificationResponse
            {
                Id = notification.Id.ToString(),
                UserId = notification.TargetUserId,
                Type = "account", 
                Title = notification.Title,
                Message = notification.Message,
                Status = "read",
                CreatedAt = notification.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                UpdatedAt = notification.ReadAt?.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse
            { 
                Error = "SERVER_ERROR", 
                Message = "Unable to update notification at this time" 
            });
        }
    }

    // DELETE: /api/notifications
    [HttpDelete]
    [ProducesResponseType(typeof(DeleteResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<ActionResult<DeleteResponse>> DeleteAll()
    {
        // [PRODUCTION]: Auth
        // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // if (userId == null) return Unauthorized(new ErrorResponse { Error = "UNAUTHORIZED", Message = "User is not authenticated" });

        // [TESTING]: Hardcoded user
        var userId = "user456"; 

        try
        {
            var userNotifications = await _context.Notifications
                .Where(n => n.TargetUserId == userId)
                .ToListAsync();

            if (userNotifications.Any())
            {
                _context.Notifications.RemoveRange(userNotifications);
                await _context.SaveChangesAsync();
            }

            return Ok(new DeleteResponse
            {
                UserId = userId,
                Message = "All notifications successfully deleted."
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse
            { 
                Error = "SERVER_ERROR", 
                Message = "Failed to delete notifications. Please try again later." 
            });
        }
    }

    // DELETE: /api/notifications/{notifId}
    [HttpDelete("{notifId}")]
    [ProducesResponseType(typeof(DeleteResponse), 200)]
    [ProducesResponseType(typeof(ErrorResponse), 404)]
    [ProducesResponseType(typeof(ErrorResponse), 500)]
    public async Task<ActionResult<DeleteResponse>> DeleteOne([FromRoute] int notifId)
    {
        // [PRODUCTION]: Auth
        // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // if (userId == null) return Unauthorized(new ErrorResponse { Error = "UNAUTHORIZED", Message = "User is not authenticated" });
        
        // [TESTING]: Hardcoded user
        var userId = "user456";

        try 
        {
            var notification = await _context.Notifications.FindAsync(notifId);

            if (notification == null) 
            {
                return NotFound(new ErrorResponse
                { 
                    Error = "NO_NOTIFICATIONS", 
                    Message = "Notification does not exist." 
                });
            }

            // [PRODUCTION]: Ownership Check
            // if (notification.TargetUserId != userId)
            // {
            //     return StatusCode(403, new ErrorResponse { Error = "ACCESS_DENIED", Message = "Permission denied" });
            // }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok(new DeleteResponse
            { 
                UserId = userId,
                NotifId = notifId,
                Message = "Notification successfully deleted." 
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new ErrorResponse
            { 
                Error = "SERVER_ERROR", 
                Message = "Failed to delete notification. Please try again later." 
            });
        }
    }
}