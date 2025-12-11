namespace NotificationService.Models;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Status { get; set; }
    // public string? ExtraData { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmailAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

}