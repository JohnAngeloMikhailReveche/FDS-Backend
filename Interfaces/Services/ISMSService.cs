using NotificationService.DTOs;

namespace NotificationService.Interfaces;

public interface ISMSService
{
    Task<int> SendSMSAsync(string userId, CreateNotificationDTO notificationDTO);
}