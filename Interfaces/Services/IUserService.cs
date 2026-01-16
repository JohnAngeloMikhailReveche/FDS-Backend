using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User?>> GetAllAsync();
    Task<string> CreateAsync(string userId, CreateUserDTO userDTO);
    Task<User?> UpdateAsync(string userId, CreateUserDTO userDTO);
    Task<bool> DeleteAsync(string userId);
}