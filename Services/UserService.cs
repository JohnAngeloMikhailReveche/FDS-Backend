using NotificationService.DTOs;
using NotificationService.Interfaces;
using NotificationService.Mapping;
using NotificationService.Models;

namespace NotificationService.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }


    public async Task<IEnumerable<UserResponseDTO>> GetAllAsync()
    {
        var users = await _userRepository.GetAllUserAsync();
        return users.Where(u => u != null).Select(u => u!.ToResponseDTO());
    }


    public async Task<string> CreateAsync(string userId, CreateUserDTO userDTO)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if (existingUser != null)
            throw new InvalidOperationException("User already exists.");

        var user = new User
        {
            Id = userId,
            Email = userDTO.Email,
            PhoneNumber = userDTO.PhoneNumber
        };

        await _userRepository.CreateUserAsync(user);
        return userId;
    }


    public async Task<User?> UpdateAsync(string userId, CreateUserDTO userDTO)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if (existingUser == null)
            return null;
        
        var success = await _userRepository.UpdateUserAsync(
            userId,
            userDTO.Email,
            userDTO.PhoneNumber
        );

        if (!success)
            throw new Exception("Failed to update user.");

        return await _userRepository.GetUserByIdAsync(userId);
    }


    public async Task<bool> DeleteAsync(string userId)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);
        if (existingUser == null)
            return false;

        return await _userRepository.DeleteUserAsync(userId);
    }
}