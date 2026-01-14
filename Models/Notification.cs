using System;
using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Type { get; set; } = "account";
        public bool IsRead { get; set; } 
        public DateTime? ReadAt { get; set; } 
        public string? Status { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Key
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
    }
}