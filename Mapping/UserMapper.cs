using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Mapping
{
    public static class UserMapping
    {
        public static UserResponseDTO ToResponseDTO(this User user)
        {
            return new UserResponseDTO
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }
    }
    
}
 
 
 
