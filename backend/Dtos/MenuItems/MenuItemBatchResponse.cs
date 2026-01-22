namespace KapeBara.MenuService.Dtos.MenuItems;

public record MenuItemBatchResponse(
    int MenuItemId,
    string MenuItemName,
    string? ImageUrl,
    int MenuItemVariantId,
    int VariantId,
    string VariantName,
    decimal Price
);
