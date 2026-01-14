using NotificationService.Models;

namespace NotificationService.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(string userId);
        Task<User> CreateUserAsync(User user);
        Task<User> GetOrCreateUserAsync(string userId, string? email = null, string? phoneNumber = null);
        Task<(string? Email, string? PhoneNumber)> GetUserContactAsync(string userId);
        Task<bool> UpdateUserAsync(string userId, string? email = null, string? phoneNumber = null);
        Task<bool> DeleteUserAsync(string userId);
    }
}
