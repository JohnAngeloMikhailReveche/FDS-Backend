using System.ComponentModel.DataAnnotations;

namespace OrderService.Models.DTO
{
    public class CancellationRequestDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 5)]
        public string Reason { get; set; } = string.Empty; 
    }
}