using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Interfaces;

public interface IEmailService
{
    Task<int> SendEmailNotificationAsync(string userId, CreateNotificationDTO notificationDTO);
}