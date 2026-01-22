namespace KapeBara.MenuService.Dtos.MenuItems;

public record MenuItemResponse(
    int Id,
    string Name,
    string? Description,
    string? ImageUrl,
    bool IsAvailable,
    int CategoryId,
    string CategoryName
);
