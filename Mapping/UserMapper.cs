using NotificationService.DTOs;
using NotificationService.Models;

namespace NotificationService.Mapping
{
    public static class UserMapping
    {
        public static ResponseUserDTO ToResponseDTO(this User user)
        {
            return new ResponseUserDTO
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }
    }
    
}
 
 
 
