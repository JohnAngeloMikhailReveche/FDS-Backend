using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Mapping
{
    public static class NotificationMapping
    {
        public static NotificationResponseDTO ToResponseDTO(this Notification notification)
        {
            return new NotificationResponseDTO
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Subject = notification.Subject,
                Body = notification.Body,
                Type = notification.Type,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                CreatedAt = notification.CreatedAt,
                UpdatedAt = notification.UpdatedAt
            };
        }
    }
}
