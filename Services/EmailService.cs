using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Build.Framework;
using NotificationService.DTOs;
using NotificationService.Helpers;
using NotificationService.Models;
using NotificationService.Interfaces;
using System.Net.Mail;

namespace NotificationService.Services;

public class EmailService : IEmailService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly GmailEmailService _gmail;

    public EmailService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        GmailEmailService gmail)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _gmail = gmail;
    }

    public async Task<int> SendEmailNotificationAsync(
        string userId, 
        CreateNotificationDTO notificationDTO)
    {
        var user = await _userRepository.GetOrCreateUserAsync(userId);
        if (user == null)
            throw new InvalidOperationException("Failed to create or retrieve user.");

        var contact = await _userRepository.GetUserContactAsync(userId);
        if (contact == null)
            throw new InvalidOperationException("User contact not found.");
        
        var (email, _) = contact.Value;

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("User email not found.");

        email = email.Trim();

        if (!MailAddress.TryCreate(email, out _))
            throw new InvalidOperationException("Invalid email address.");

         await _gmail.SendEmailAsync(
            email,
            notificationDTO.Subject,
            notificationDTO.Body
        );

        var notification = new Notification
        {
            UserId = userId,
            Subject = notificationDTO.Subject,
            Body = notificationDTO.Body,
            Type = "Email",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsRead = false
        };

       
        var notificationId = await _notificationRepository.AddNotificationAsync(notification);
        return notificationId;
    }
}