using System;
using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public string TargetUserId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = "account";

        public bool IsRead { get; set; } 

        public DateTime? ReadAt { get; set; } 

        public string? Status { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string? ExtraData { get; set; }

        public string? PhoneNumber { get; set; }

        public string? EmailAddress { get; set; }
    }
}