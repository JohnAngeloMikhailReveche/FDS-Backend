// FoodDeliverySystem.Domain/Entities/Admin.cs
namespace FoodDeliverySystem.Domain.Entities
{
    public class Admin : User
    {
        // Admin-specific properties can be added here
    }
}

// FoodDeliverySystem.Domain/Entities/Rider.cs
namespace FoodDeliverySystem.Domain.Entities
{
    public class Rider : User
    {
        public string ContactNumber { get; set; } = string.Empty;
        public string MotorcycleModel { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
    }
}

// FoodDeliverySystem.Domain/Entities/Customer.cs
namespace FoodDeliverySystem.Domain.Entities
{
    public class Customer : User
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}