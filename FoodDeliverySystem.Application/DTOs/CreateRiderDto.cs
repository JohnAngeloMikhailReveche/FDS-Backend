namespace FoodDeliverySystem.Application.DTOs
{
    public class CreateRiderDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string MotorcycleModel { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}