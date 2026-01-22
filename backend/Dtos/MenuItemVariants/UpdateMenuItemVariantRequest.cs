using System.ComponentModel.DataAnnotations;

namespace KapeBara.MenuService.Dtos.MenuItemVariants;

public record UpdateMenuItemVariantRequest(
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    decimal Price
);
