using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NotificationService.Models;
using System.Data;

namespace NotificationService.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly string _connectionString; 
    public NotificationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("NotificationDatabase");
    }

    public async Task<IEnumerable<Notification>> GetAllNotificationsAsync()
    {
        var notifications = new List<Notification>();
        using var conn = new SqlConnection(_connectionString);
        var cmd = new SqlCommand("SELECT * FROM Notifications", conn);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            notifications.Add(new Notification
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                Type = reader.GetString(2),
                Title = reader.GetString(3),
                Message = reader.GetString(4),
                Status = reader.GetString(5),
                PhoneNumber = reader.IsDBNull(6) ? null : reader.GetString(6),
                EmailAddress = reader.IsDBNull(7) ? null : reader.GetString(7),
                CreatedAt = reader.GetDateTime(8),
                UpdatedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
            });
        }

        return notifications;
    }

    public async Task<Notification> GetNotificationByIdAsync(int id)
    {
        Notification? notification = null;
        using var conn = new SqlConnection(_connectionString);
        var cmd = new SqlCommand("SELECT * FROM Notifications WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            notification = new Notification
            {
                Id = reader.GetInt32(0),
                UserId = reader.GetInt32(1),
                Type = reader.GetString(2),
                Title = reader.GetString(3),
                Message = reader.GetString(4),
                Status = reader.GetString(5),
                PhoneNumber = reader.IsDBNull(6) ? null : reader.GetString(6),
                EmailAddress = reader.IsDBNull(7) ? null : reader.GetString(7),
                CreatedAt = reader.GetDateTime(8),
                UpdatedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
            };
        }

        return notification;
    }

    public async Task AddNotificationAsync(Notification notification)
    {
        using var conn = new SqlConnection(_connectionString);
        var cmd = new SqlCommand("INSERT INTO Notifications (UserId, Type, Title, Message, Status, PhoneNumber, EmailAddress, CreatedAt) VALUES (@UserId, @Type, @Title, @Message, @Status, @PhoneNumber, @EmailAddress, @CreatedAt)", conn);
        cmd.Parameters.AddWithValue(@"UserId", notification.UserId);
        cmd.Parameters.AddWithValue("@Type", notification.Type);
        cmd.Parameters.AddWithValue("@Title", notification.Title);
        cmd.Parameters.AddWithValue("@Message", notification.Message);
        cmd.Parameters.AddWithValue("@Status", notification.Status);
        cmd.Parameters.AddWithValue("@PhoneNumber", (object?)notification.PhoneNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EmailAddress", (object?)notification.EmailAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedAt", notification.CreatedAt);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
        return;
    }

    public async Task UpdateNotificationAsync(Notification notification)
    {
        using var conn = new SqlConnection(_connectionString);
        var cmd = new SqlCommand("UPDATE Notifications SET UserId = @UserId, Type = @Type, Title = @Title, Message = @Message, Status = @Status, PhoneNumber = @PhoneNumber, EmailAddress = @EmailAddress, UpdatedAt = @UpdatedAt WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", notification.Id);
        cmd.Parameters.AddWithValue("@UserId", notification.UserId);
        cmd.Parameters.AddWithValue("@Type", notification.Type);
        cmd.Parameters.AddWithValue("@Title", notification.Title);
        cmd.Parameters.AddWithValue("@Message", notification.Message);
        cmd.Parameters.AddWithValue("@Status", notification.Status);
        cmd.Parameters.AddWithValue("@PhoneNumber", (object?)notification.PhoneNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@EmailAddress", (object?)notification.EmailAddress ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UpdatedAt", (object?)notification.UpdatedAt ?? DBNull.Value);
        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();

        return;
    }

    public async Task DeleteNotificationAsync(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        var cmd = new SqlCommand("DELETE FROM Notifications WHERE Id = @Id", conn);
        cmd.Parameters.AddWithValue("@Id", id);

        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();

        return;
    }
    
}


