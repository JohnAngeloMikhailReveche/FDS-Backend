using KapeBara.MenuService.Dtos.MenuItemVariants;

namespace KapeBara.MenuService.Dtos.MenuItems;

public record MenuItemDetailResponse(
    int Id,
    string Name,
    string? Description,
    string? ImageUrl,
    bool IsAvailable,
    int CategoryId,
    string CategoryName,
    List<MenuItemVariantResponse> Variants
);
