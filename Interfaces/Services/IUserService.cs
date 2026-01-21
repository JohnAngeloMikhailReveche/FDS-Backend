using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Interfaces;

public interface IUserService
{
    Task<IEnumerable<ResponseUserDTO>> GetAllAsync();
    Task<string> CreateAsync(string userId, CreateUserDTO userDTO);
    Task<ResponseUserDTO?> UpdateAsync(string userId, CreateUserDTO userDTO);
    Task<bool> DeleteAsync(string userId);
}