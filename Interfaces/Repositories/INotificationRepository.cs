using NotificationService.Models;

namespace NotificationService.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetAllNotificationsAsync(string targetUserId);
    Task<Notification?> GetNotificationAsync(string targetUserId, int id);
    Task<int> AddNotificationAsync(Notification notification);
    Task<int> UpdateNotificationAsync(Notification notification);
    Task<bool> MarkAllAsReadAsync(string targetUserId);
    Task<bool> MarkAsReadAsync(string targetUserId, int id);
    Task<bool> DeleteAllNotificationsAsync(string targetUserId);
    Task<bool> DeleteNotificationAsync(string targetUserId, int id);
}