using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class Notification
    {
        [Key]
        public string? Id { get; set; } // Changed to String to match UUID/NVARCHAR(36)

        public string? UserId { get; set; }

        public string? Type { get; set; } // "in-app", "sms", "email"

        public string? Title { get; set; }

        public string? Message { get; set; }

        public string? Status { get; set; } // "unread", "read"

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? ExtraData { get; set; }

        public string? PhoneNumber { get; set; }

        public string? EmailAddress { get; set; }
    }
}