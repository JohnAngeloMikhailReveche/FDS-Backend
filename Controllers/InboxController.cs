using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Models;
using NotificationService.DTOs;
using NotificationService.Mapping;
using System.Security.Claims;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using NotificationService.Interfaces;

namespace NotificationService.Controllers;

// [Authorize]
[Route("api/notifications/")]
[ApiController]
public class InboxController : ControllerBase
{
    private readonly IInboxService _inboxService; 

    public InboxController(IInboxService inboxService)
    {
        _inboxService = inboxService;
    }

    private string GetUserId()
    {
        // return User.FindFirstValue(ClaimTypes.NameIdentifier)
        //     ?? User.FindFirstValue("sub")
        //     ?? throw new UnauthorizedAccessException("User ID not found in token.");

        return "123";
    }


    /// Get all notifications for user
    [HttpGet()]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = GetUserId();

        var notification = await _inboxService.GetAllAsync(userId);
        return Ok(notification);
    }


    /// Get a specific notification
    [HttpGet("{id}")]
    public async Task<IActionResult> GetNotification(int id)
    {            
        var userId = GetUserId();

        var notification = await _inboxService.GetByIdAsync(userId, id);
        if (notification == null)
            return NotFound();
        
        return Ok(notification);
    }


    /// Send notification in-app and save notification to inbox
    [HttpPost("send-message")]
    public async Task<IActionResult> SendInAppNotification(CreateNotificationDTO notificationDTO)
    {
        var userId = GetUserId();

        var notification = await _inboxService.CreateAsync(userId, notificationDTO);
        return CreatedAtAction(
            nameof(GetNotification),
            new { id = notification.Id },
            notification );
    }


    /// Mark all notifications as read
    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();

        var success = await _inboxService.MarkAllAsReadAsync(userId);
        if (!success)
            return NotFound(new { message = "No notifications to mark as read." });

        return Ok(new { message = "All notifications marked as read." });
    }


    /// Mark a specific notification as read
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetUserId();
        
        var success = await _inboxService.MarkAsReadAsync(userId, id);
        if (!success)
            return NotFound(new { message = "Notification not found." });

        return Ok(new { message = "Notification marked as read.", id });
    }


    /// Delete all notifications for user
    [HttpDelete("remove-all")]
    public async Task<IActionResult> DeleteAllNotifications()
    {
        var userId = GetUserId();

        var success = await _inboxService.DeleteAllAsync(userId);
        if (!success)
            return NotFound(new { message = "No notifications to be deleted."});

        return Ok(new { message = "All notifications deleted."});
    }


    /// Delete a specific notification
    [HttpDelete("{id}/remove")]
    public async Task<IActionResult> DeleteNotification(int id)
    {  
        var userId = GetUserId();

        var success = await _inboxService.DeleteAsync(userId, id);
        if (!success)
            return NotFound(new { message = "Failed to delete notification." });
        
        return Ok(new { message = "Notification deleted."});
    }
}
