using NotificationService.Models;

namespace NotificationService.Repositories;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetAllNotificationsAsync();
    Task<Notification> GetNotificationByIdAsync(int id);
    Task AddNotificationAsync(Notification notification);
    Task UpdateNotificationAsync(Notification notification);
    Task DeleteNotificationAsync(int id);
}