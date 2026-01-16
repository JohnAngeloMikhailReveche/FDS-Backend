using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Interfaces;

public interface IInboxService
{
    Task<IEnumerable<NotificationResponseDTO>> GetAllAsync(string userId);
    Task<NotificationResponseDTO?> GetByIdAsync(string userId, int id);
    Task<Notification> CreateAsync(string userId, CreateNotificationDTO notificationDTO);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<bool> MarkAsReadAsync(string userId, int id);
    Task<bool> DeleteAllAsync(string userId);
    Task<bool> DeleteAsync(string userId, int id);
}