using System.ComponentModel.DataAnnotations;

namespace KapeBara.MenuService.Dtos.MenuItems;

public record MenuItemBatchRequest(
    [Required]
    [MinLength(1)]
    List<int> MenuItemVariantIds
);
