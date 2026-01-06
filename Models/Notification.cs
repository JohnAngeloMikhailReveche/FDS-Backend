using System;

namespace NotificationService.Models;

public class Notification
{
    public int Id { get; set; }
     // KEPT LOCAL: Renamed (TargetUserID) to match Controller/Auth logic (String ID)
    public string TargetUserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "account"; 
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? Status { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmailAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; } 
}