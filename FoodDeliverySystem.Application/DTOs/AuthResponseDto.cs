// ============================================================================
// FoodDeliverySystem.Application/DTOs/AuthResponseDto.cs
// Response DTO for authentication endpoints
// ============================================================================

namespace FoodDeliverySystem.Application.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;  // Added FullName
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}