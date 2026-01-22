namespace KapeBara.MenuService.Dtos.MenuItemVariants;

public record MenuItemVariantResponse(
    int Id,
    int VariantId,
    string VariantName,
    decimal Price
);
