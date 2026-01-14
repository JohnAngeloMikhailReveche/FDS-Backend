using System;

namespace NotificationService.DTOs
{
    public class CreateNotificationDTO
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
}
