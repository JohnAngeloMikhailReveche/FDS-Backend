using NotificationService.DTOs;
using NotificationService.Interfaces;
using NotificationService.Models;
using NotificationService.Helpers;

namespace NotificationService.Services;

public class SMSService : ISMSService
{
    private readonly SemaphoreSMSService _semaphoreSMS;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;

    public SMSService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        SemaphoreSMSService semaphoreSMS)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _semaphoreSMS = semaphoreSMS;
    }


    public async Task<int> SendSMSAsync(
        string userId,
        CreateNotificationDTO notificationDTO)
    {
        var user = await _userRepository.GetOrCreateUserAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Failed to create or retrive user.");

        var contact = await _userRepository.GetUserContactAsync(userId);
        if (contact == null)
            throw new InvalidOperationException("User contact not found.");

        if (string.IsNullOrWhiteSpace(contact?.PhoneNumber))
            throw new InvalidOperationException("User phone number not found.");

        var phoneNumber = contact.Value.PhoneNumber?.Trim();
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new InvalidOperationException("User phone number is empty.");
            
        if (phoneNumber.StartsWith("0"))
            phoneNumber = "+63" + phoneNumber[1..];

        var smsResponse = await _semaphoreSMS.SendSMSAsync(phoneNumber, notificationDTO.Body);
        if (string.IsNullOrEmpty(smsResponse))
            throw new InvalidOperationException("Cannot send a message.");

        var notification = new Notification
        {
            UserId = userId,
            Subject = notificationDTO.Subject,
            Body = notificationDTO.Body,
            Type = "SMS",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsRead = false
        };

        var notificationId = await _notificationRepository.AddNotificationAsync(notification);
        return notificationId;
    }
}