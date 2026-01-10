using System;

namespace NotificationService.DTOs
{
    public class CreateNotificationDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
