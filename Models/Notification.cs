using System;

namespace NotificationService.Models;

public class Notification
{
    public int Id { get; set; }
    
    // Fixed: Renamed from UserId to TargetUserId to match the Controller
    public string TargetUserId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    
    public string Message { get; set; } = string.Empty;
    
    // Optional: If you want to store Type in the DB, keep this. 
    // Initializing it avoids the "required" warning.
    public string Type { get; set; } = "account"; 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsRead { get; set; }
    
    public DateTime? ReadAt { get; set; }
}