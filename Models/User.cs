using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class User
    {
       [Key]
       public string Id { get; set; } = string.Empty;
       public string? Email { get; set; } 
       public string? PhoneNumber { get; set; }

       public ICollection<Notification>? Notifications { get; }
    }
}