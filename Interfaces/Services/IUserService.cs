using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponseDTO>> GetAllAsync();
    Task<string> CreateAsync(string userId, CreateUserDTO userDTO);
    Task<UserResponseDTO?> UpdateAsync(string userId, CreateUserDTO userDTO);
    Task<bool> DeleteAsync(string userId);
}