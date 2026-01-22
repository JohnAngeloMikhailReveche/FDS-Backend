using System.ComponentModel.DataAnnotations;

namespace KapeBara.MenuService.Dtos.MenuItems;

public record UpdateMenuItemRequest(
    [Required]
    [MaxLength(150)]
    string Name,

    [MaxLength(1000)]
    string? Description,

    [Required]
    int CategoryId,

    bool IsAvailable
);
