namespace NotificationService.Models;

public class NotificationRequest
{
    public string TargetUserId { get; set; }
    public List<string> Channels { get; set; } 
    public string Title { get; set; }
    public string Message { get; set; }
}

public class MarkReadRequest
{
    public bool IsRead { get; set; }
}