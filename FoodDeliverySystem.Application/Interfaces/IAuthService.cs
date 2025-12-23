using FoodDeliverySystem.Application.DTOs;
using FoodDeliverySystem.Domain.Enums;

namespace FoodDeliverySystem.Application.Interfaces
{
    public interface IAuthService
    {
        // Authentication
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerDto registerDto);

        // Account Creation (Admin only) - Updated to 3 parameters
        Task<AuthResponseDto> CreateAdminAsync(CreateAdminDto adminDto, UserRole creatorRole, string creatorEmail);
        Task<AuthResponseDto> CreateRiderAsync(CreateRiderDto riderDto, UserRole creatorRole, string creatorEmail);

        // Account Management (Admin only) - Fixed method names
        Task<bool> DeleteAccountAsync(string email, UserRole deleterRole, string deleterEmail);
        Task<List<UserInfoDto>> GetAllUsersAsync(UserRole requesterRole);
    }
}