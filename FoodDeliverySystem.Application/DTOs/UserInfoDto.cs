// FoodDeliverySystem.Application/DTOs/UserInfoDto.cs
namespace FoodDeliverySystem.Application.DTOs
{
    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        // Additional fields based on role
        public string? ContactNumber { get; set; }
        public string? MotorcycleModel { get; set; }
        public string? PlateNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}

// FoodDeliverySystem.Application/DTOs/DeleteAccountDto.cs
namespace FoodDeliverySystem.Application.DTOs
{
    public class DeleteAccountDto
    {
        public string Email { get; set; } = string.Empty;
    }
}