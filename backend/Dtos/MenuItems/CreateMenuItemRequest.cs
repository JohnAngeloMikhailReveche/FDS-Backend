using System.ComponentModel.DataAnnotations;

namespace KapeBara.MenuService.Dtos.MenuItems;

public class CreateMenuItemRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public bool IsAvailable { get; set; } = true;

    public string? ImageUrl { get; set; } // scalar field
    // Add other scalar fields as needed
}
