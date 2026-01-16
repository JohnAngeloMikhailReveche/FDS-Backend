// ============================================================================
// Password Management DTOs
// Location: FoodDeliverySystem.Application/DTOs/
// ============================================================================

namespace FoodDeliverySystem.Application.DTOs
{
    /// <summary>
    /// DTO for requesting password reset
    /// Used in: POST /api/Auth/forgot-password
    /// </summary>
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for resetting password (public - no auth required)
    /// Used in: POST /api/Auth/reset-password
    /// </summary>
    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for changing password (authenticated users only)
    /// Used in: POST /api/Auth/change-password
    /// </summary>
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}