using NotificationService.Models;

namespace NotificationService.Repositories;

public interface INotificationRepository
{
    Task<IEnumerable<Notification?>> GetAllNotificationsAsync(int userId);
    Task<Notification?> GetNotificationAsync(int userId, int notifId);
    Task<int> AddNotificationAsync(Notification notification);
    Task UpdateNotificationAsync(Notification notification);
    Task DeleteAllNotificationsAsync(int userId);
    Task DeleteNotificationAsync(int userId, int notifId);
}