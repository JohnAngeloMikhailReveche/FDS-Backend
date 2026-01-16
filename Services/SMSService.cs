using NotificationService.DTOs;
using NotificationService.Interfaces;
using NotificationService.Models;

namespace NotificationService.Services;

public class SMSService : ISMSService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;

    public SMSService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
    }

    public async Task<int> SendSMSAsync(
        string userId,
        CreateNotificationDTO notificationDTO)
    {
        await _userRepository.GetOrCreateUserAsync(userId);

        // SMS Logic goes here

        var notification = new Notification
        {
            UserId = userId,
            Subject = notificationDTO.Subject,
            Body = notificationDTO.Body,
            Type = "SMS",
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        var id = await _notificationRepository.AddNotificationAsync(notification);
        return id;
    }
}