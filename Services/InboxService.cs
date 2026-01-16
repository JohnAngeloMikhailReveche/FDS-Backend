using NotificationService.DTOs;
using NotificationService.Mapping;
using NotificationService.Models;
using NotificationService.Interfaces;

namespace NotificationService.Services;

public class InboxService : IInboxService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;

    public InboxService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
    }


    public async Task<IEnumerable<NotificationResponseDTO>> GetAllAsync(string userId)
    {
        var notifications = await _notificationRepository.GetAllNotificationsAsync(userId);
        return notifications.Select(n => n.ToResponseDTO());      
    }    


    public async Task<NotificationResponseDTO?> GetByIdAsync(string userId, int id)
    {
        var notification = await _notificationRepository.GetNotificationAsync(userId, id);
        return notification?.ToResponseDTO();
    }


    public async Task<Notification> CreateAsync(string userId, CreateNotificationDTO notificationDTO)
    {
        await _userRepository.GetOrCreateUserAsync(userId);

        var notification = new Notification
        {
            UserId = userId,
            Subject = notificationDTO.Subject,
            Body = notificationDTO.Body,
            Type = "In-App",
            CreatedAt = DateTime.UtcNow,
            ReadAt = null,
            UpdatedAt = null,
            IsRead = false
        };

        var id = await _notificationRepository.AddNotificationAsync(notification);
        notification.Id = id;

        return notification;
    }


    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        return await _notificationRepository.MarkAllAsReadAsync(userId);
    }


    public async Task<bool> MarkAsReadAsync(string userId, int id)
    {
        var notification = await _notificationRepository.GetNotificationAsync(userId, id);
        if (notification == null)
            return false;
        
        return await _notificationRepository.MarkAsReadAsync(userId, id);
    }


    public async Task<bool> DeleteAllAsync(string userId)
    {
        return await _notificationRepository.DeleteAllNotificationsAsync(userId);
    }


    public async Task<bool> DeleteAsync(string userId, int id)
    {
        var notification = await _notificationRepository.GetNotificationAsync(userId, id);
        if (notification == null)
            return false;

        return await _notificationRepository.DeleteNotificationAsync(userId, id);
    }
}