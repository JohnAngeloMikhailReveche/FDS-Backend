namespace NotificationService.DTOs
{
    public class UserResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
