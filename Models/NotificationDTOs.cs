using System.Text.Json.Serialization;

namespace NotificationService.Models;

// --- REQUEST OBJECTS ---

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

public class CreateNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public object? ExtraData { get; set; } 
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

// --- NEW RESPONSE OBJECTS (Required for the new Controller) ---

public class NotificationResponse
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Status { get; set; }
    public string CreatedAt { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UpdatedAt { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ExtraData { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; }
    public string Message { get; set; }
}

public class DeleteResponse
{
    public string UserId { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? NotifId { get; set; }
    
    public string Message { get; set; }
}

public class NotificationListResponse
{
    public IEnumerable<NotificationResponse> Notifications { get; set; }
}