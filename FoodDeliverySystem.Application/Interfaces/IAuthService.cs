// ============================================================================
// FoodDeliverySystem.Application/Interfaces/IAuthService.cs
// UPDATED: Added password management methods
// ============================================================================

using FoodDeliverySystem.Application.DTOs;
using FoodDeliverySystem.Domain.Enums;

namespace FoodDeliverySystem.Application.Interfaces
{
    public interface IAuthService
    {
        // ========== AUTHENTICATION ==========
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerDto registerDto);

        // ========== ACCOUNT CREATION (Admin only) ==========
        Task<AuthResponseDto> CreateAdminAsync(CreateAdminDto adminDto, UserRole creatorRole, string creatorEmail);
        Task<AuthResponseDto> CreateRiderAsync(CreateRiderDto riderDto, UserRole creatorRole, string creatorEmail);

        // ========== ACCOUNT MANAGEMENT (Admin only) ==========
        Task<bool> DeleteAccountAsync(string email, UserRole deleterRole, string deleterEmail);
        Task<List<UserInfoDto>> GetAllUsersAsync(UserRole requesterRole);

        // ========== PASSWORD MANAGEMENT ========== 
        // 👇 THESE ARE THE NEW METHODS
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto, string userEmail);
    }
}