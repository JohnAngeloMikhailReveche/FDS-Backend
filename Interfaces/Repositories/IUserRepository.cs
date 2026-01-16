using NotificationService.Models;

namespace NotificationService.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User?>> GetAllUserAsync();
    Task<User?> GetUserByIdAsync(string userId);
    Task<(string? Email, string? PhoneNumber)?> GetUserContactAsync(string userId);
    Task<User?> CreateUserAsync(User user);
    Task<User> GetOrCreateUserAsync(string userId, string? email = null, string? phoneNumber = null);
    Task<bool> UpdateUserAsync(string userId, string? email = null, string? phoneNumber = null);
    Task<bool> DeleteUserAsync(string userId);
}
