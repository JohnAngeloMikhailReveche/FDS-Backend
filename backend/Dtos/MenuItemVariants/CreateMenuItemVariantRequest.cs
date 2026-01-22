using System.ComponentModel.DataAnnotations;

namespace KapeBara.MenuService.Dtos.MenuItemVariants;

public record CreateMenuItemVariantRequest(
    [Required]
    int VariantId,

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    decimal Price
);
