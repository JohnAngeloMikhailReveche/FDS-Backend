using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Extensions
{
    public static class NotificationExtensions
    {
        public static NotificationResponseDTO ToResponseDTO(this Notification notification)
        {
            return new NotificationResponseDTO
            {
                Id = notification.Id,
                TargetUserId = notification.TargetUserId,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                Status = notification.Status,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                CreatedAt = notification.CreatedAt,
                UpdatedAt = notification.UpdatedAt
            };
        }
    }
}
