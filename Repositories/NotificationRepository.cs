using Dapper;
using Microsoft.Data.SqlClient;
using NotificationService.Models;
using NotificationService.Interfaces;
using System.Data;
using System.Runtime.CompilerServices;

namespace NotificationService.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly string _connectionString; 
    public NotificationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("NotificationDatabase") 
            ?? throw new InvalidOperationException("Connection string 'NotificationDatabase' is not configured.");
    }


    public async Task<IEnumerable<Notification>> GetAllNotificationsAsync(string userId)
    {
        var notifications = new List<Notification>();

        using var conn = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.usp_GetAllNotifications", conn);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@UserId", userId);

        await conn.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notifications.Add(new Notification
            {
                Id = reader.GetInt32("Id"),
                Subject = reader.GetString("Subject"),
                Body = reader.GetString("Body"),
                Type = reader.GetString("Type"),
                IsRead = reader.GetBoolean("IsRead"),
                ReadAt = reader.IsDBNull("ReadAt") ? null : reader.GetDateTime("ReadAt"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UserId = reader.GetString("UserId") 
            });
        }

        return notifications;
    }


    public async Task<Notification?> GetNotificationAsync(string userId, int id)
    {
        using var conn = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.usp_GetNotification", conn);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@Id", id);

        await conn.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Notification
            {
                Id = reader.GetInt32("Id"),
                Subject = reader.GetString("Subject"),
                Body = reader.GetString("Body"),
                Type = reader.GetString("Type"),
                IsRead = reader.GetBoolean("IsRead"),
                ReadAt = reader.IsDBNull("ReadAt") ? null : reader.GetDateTime("ReadAt"),
                CreatedAt = reader.GetDateTime("CreatedAt"),
                UserId = reader.GetString("UserId")
            };
        }

        return null;
    }


    public async Task<int> AddNotificationAsync(Notification notification)
    {
        using var conn = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.usp_AddNotification", conn);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@UserId", notification.UserId);
        command.Parameters.AddWithValue("@Type", notification.Type);
        command.Parameters.AddWithValue("@Subject", notification.Subject);
        command.Parameters.AddWithValue("@Body", notification.Body);
        command.Parameters.AddWithValue("@IsRead", notification.IsRead);
        command.Parameters.AddWithValue("@CreatedAt", notification.CreatedAt);
        command.Parameters.AddWithValue("@ReadAt", notification.ReadAt ?? (object)DBNull.Value);

        await conn.OpenAsync();
    
        var result = await command.ExecuteScalarAsync();
        if (result == null)
            throw new InvalidOperationException("Failed to add notification: no ID returned.");
        int id = (int)result;
        return id;
    }


    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        using var conn = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.usp_MarkAllAsRead", conn);

        command.CommandType = CommandType.StoredProcedure;
        
        var now = DateTime.UtcNow;
        command.Parameters.AddWithValue("@ReadAt", now);
        command.Parameters.AddWithValue("@UserId", userId);

        await conn.OpenAsync();

        int affectedRows = await command.ExecuteNonQueryAsync();
        return affectedRows > 0;
    }


    public async Task<bool> MarkAsReadAsync(string userId, int id)
    {
        using var conn = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.usp_MarkAsRead", conn);

        command.CommandType = CommandType.StoredProcedure;
        
        var now = DateTime.UtcNow;
        command.Parameters.AddWithValue("@ReadAt", now);
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@UserId", userId);

        await conn.OpenAsync();
    
        int affectedRows = await command.ExecuteNonQueryAsync();
        return affectedRows > 0;
    }


    public async Task<bool> DeleteAllNotificationsAsync(string userId)
    {
        using var conn = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.usp_DeleteAllNotifications", conn);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@UserId", userId);
    
        await conn.OpenAsync();

        int affectedRows = await command.ExecuteNonQueryAsync(); 
        return affectedRows > 0;
    }


    public async Task<bool> DeleteNotificationAsync(string userId, int id)
    {
        using var conn = new SqlConnection(_connectionString);
        using var command = new SqlCommand("dbo.usp_DeleteNotification", conn);

        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@Id", id);
        
        await conn.OpenAsync();
        
        var affectedRows = await command.ExecuteNonQueryAsync();
        return affectedRows > 0;
    }
}