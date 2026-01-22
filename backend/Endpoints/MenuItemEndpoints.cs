using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using KapeBara.MenuService.Data;
using KapeBara.MenuService.Dtos.MenuItems;
using KapeBara.MenuService.Dtos.MenuItemVariants;
using KapeBara.MenuService.Models;

namespace KapeBara.MenuService.Endpoints;

public static class MenuItemEndpoints
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];

    public static RouteGroupBuilder MapMenuItemEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/menu-items")
            .WithTags("Menu Items");

        // GET: /api/menu-items
        group.MapGet("/", GetAllMenuItems)
            .WithName("GetAllMenuItems")
            .WithSummary("Get all menu items")
            .WithDescription("Retrieves all menu items with optional search by name and filter by category.");

        // GET: /api/menu-items/{id}
        group.MapGet("/{id:int}", GetMenuItemById)
            .WithName("GetMenuItemById")
            .WithSummary("Get menu item by ID")
            .WithDescription("Retrieves a specific menu item with its variants and prices.");

        // POST: /api/menu-items
        group.MapPost("/", CreateMenuItem)
            .WithName("CreateMenuItem")
            .WithSummary("Create a new menu item")
            .WithDescription("Creates a new menu item in the specified category.");

        // PUT: /api/menu-items/{id}
        group.MapPut("/{id:int}", UpdateMenuItem)
            .WithName("UpdateMenuItem")
            .WithSummary("Update a menu item")
            .WithDescription("Updates an existing menu item by its ID.");

        // PATCH: /api/menu-items/{id}/availability
        group.MapPatch("/{id:int}/availability", UpdateMenuItemAvailability)
            .WithName("UpdateMenuItemAvailability")
            .WithSummary("Enable or disable a menu item")
            .WithDescription("Updates the availability status of a menu item (enable/disable).");

        // DELETE: /api/menu-items/{id}
        group.MapDelete("/{id:int}", DeleteMenuItem)
            .WithName("DeleteMenuItem")
            .WithSummary("Delete a menu item")
            .WithDescription("Deletes a menu item and all its associated variants.");

        // POST: /api/menu-items/batch
        group.MapPost("/batch", GetMenuItemsByBatch)
            .WithName("GetMenuItemsByBatch")
            .WithSummary("Get menu items by batch of variant IDs")
            .WithDescription("Retrieves menu item details for a list of menu item variant IDs. Used by Order Service.");

        // POST: /api/menu-items/{id}/image
        group.MapPost("/{id:int}/image", UploadMenuItemImage)
            .WithName("UploadMenuItemImage")
            .WithSummary("Upload menu item image")
            .WithDescription("Uploads an image for a menu item. Allowed formats: .jpg, .jpeg, .png")
            .DisableAntiforgery();

        // GET: /api/menu-items/suggestions
        // group.MapGet("/suggestions", GetMenuItemSuggestions)
        //     .WithName("GetMenuItemSuggestions")
        //     .WithSummary("Get menu item name suggestions")
        //     .WithDescription("Returns a list of menu item names matching the search term.");

        return group;
    }

    private static async Task<Ok<List<MenuItemResponse>>> GetAllMenuItems(
        string? search,
        int? categoryId,
        MenuDbContext db)
    {
        var query = db.MenuItems
            .Include(m => m.Category)
            .AsQueryable();

        // Filter by search term (name only)
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Name.Contains(search));
        }

        // Filter by category
        if (categoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == categoryId.Value);
        }

        var menuItems = await query
            .OrderBy(m => m.Category.Name)
            .ThenBy(m => m.Name)
            .Select(m => new MenuItemResponse(
                m.Id,
                m.Name,
                m.Description,
                m.ImageUrl,
                m.IsAvailable,
                m.CategoryId,
                m.Category.Name
            ))
            .ToListAsync();

        return TypedResults.Ok(menuItems);
    }

    private static async Task<Results<Ok<MenuItemDetailResponse>, NotFound<object>>> GetMenuItemById(int id, MenuDbContext db)
    {
        var menuItem = await db.MenuItems
            .Include(m => m.Category)
            .Include(m => m.MenuItemVariants)
                .ThenInclude(miv => miv.Variant)
            .Where(m => m.Id == id)
            .FirstOrDefaultAsync();

        if (menuItem is null)
        {
            return TypedResults.NotFound<object>(new { message = $"Menu item with ID {id} not found." });
        }

        var response = new MenuItemDetailResponse(
            menuItem.Id,
            menuItem.Name,
            menuItem.Description,
            menuItem.ImageUrl,
            menuItem.IsAvailable,
            menuItem.CategoryId,
            menuItem.Category.Name,
            menuItem.MenuItemVariants
                .OrderBy(miv => miv.Variant.Name)
                .Select(miv => new MenuItemVariantResponse(
                    miv.Id,
                    miv.VariantId,
                    miv.Variant.Name,
                    miv.Price
                ))
                .ToList()
        );

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Created<MenuItemResponse>, BadRequest<object>>> CreateMenuItem(CreateMenuItemRequest request, MenuDbContext db)
    {
        // Verify category exists
        var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return TypedResults.BadRequest<object>(new { message = $"Category with ID {request.CategoryId} not found." });
        }

        var menuItem = new MenuItem
        {
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            IsAvailable = request.IsAvailable
        };

        db.MenuItems.Add(menuItem);
        await db.SaveChangesAsync();

        // Reload with category
        await db.Entry(menuItem).Reference(m => m.Category).LoadAsync();

        var response = new MenuItemResponse(
            menuItem.Id,
            menuItem.Name,
            menuItem.Description,
            menuItem.ImageUrl,
            menuItem.IsAvailable,
            menuItem.CategoryId,
            menuItem.Category.Name
        );

        return TypedResults.Created($"/api/menu-items/{menuItem.Id}", response);
    }

    private static async Task<Results<Ok<MenuItemResponse>, NotFound<object>, BadRequest<object>>> UpdateMenuItem(int id, UpdateMenuItemRequest request, MenuDbContext db)
    {
        var menuItem = await db.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menuItem is null)
        {
            return TypedResults.NotFound<object>(new { message = $"Menu item with ID {id} not found." });
        }

        // Verify category exists if changing
        if (menuItem.CategoryId != request.CategoryId)
        {
            var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
            {
                return TypedResults.BadRequest<object>(new { message = $"Category with ID {request.CategoryId} not found." });
            }
        }

        menuItem.Name = request.Name;
        menuItem.Description = request.Description;
        menuItem.CategoryId = request.CategoryId;
        menuItem.IsAvailable = request.IsAvailable;

        await db.SaveChangesAsync();

        // Reload category if changed
        await db.Entry(menuItem).Reference(m => m.Category).LoadAsync();

        var response = new MenuItemResponse(
            menuItem.Id,
            menuItem.Name,
            menuItem.Description,
            menuItem.ImageUrl,
            menuItem.IsAvailable,
            menuItem.CategoryId,
            menuItem.Category.Name
        );

        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<MenuItemResponse>, NotFound<object>>> UpdateMenuItemAvailability(int id, bool isAvailable, MenuDbContext db)
    {
        var menuItem = await db.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (menuItem is null)
        {
            return TypedResults.NotFound<object>(new { message = $"Menu item with ID {id} not found." });
        }

        menuItem.IsAvailable = isAvailable;

        await db.SaveChangesAsync();

        var response = new MenuItemResponse(
            menuItem.Id,
            menuItem.Name,
            menuItem.Description,
            menuItem.ImageUrl,
            menuItem.IsAvailable,
            menuItem.CategoryId,
            menuItem.Category.Name
        );

        return TypedResults.Ok(response);
    }

    private static async Task<Results<NoContent, NotFound<object>>> DeleteMenuItem(int id, MenuDbContext db)
    {
        var menuItem = await db.MenuItems.FindAsync(id);

        if (menuItem is null)
        {
            return TypedResults.NotFound<object>(new { message = $"Menu item with ID {id} not found." });
        }

        db.MenuItems.Remove(menuItem);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    private static async Task<Ok<List<MenuItemBatchResponse>>> GetMenuItemsByBatch(MenuItemBatchRequest request, MenuDbContext db)
    {
        var menuItemVariants = await db.MenuItemVariants
            .Include(miv => miv.MenuItem)
            .Include(miv => miv.Variant)
            .Where(miv => request.MenuItemVariantIds.Contains(miv.Id))
            .Select(miv => new MenuItemBatchResponse(
                miv.MenuItemId,
                miv.MenuItem.Name,
                miv.MenuItem.ImageUrl,
                miv.Id,
                miv.VariantId,
                miv.Variant.Name,
                miv.Price
            ))
            .ToListAsync();

        return TypedResults.Ok(menuItemVariants);
    }

    private static async Task<Results<Ok<object>, NotFound<object>, BadRequest<object>>> UploadMenuItemImage(
        int id,
        IFormFile file,
        MenuDbContext db,
        IWebHostEnvironment env)
    {
        var menuItem = await db.MenuItems.FindAsync(id);

        if (menuItem is null)
        {
            return TypedResults.NotFound<object>(new { message = $"Menu item with ID {id} not found." });
        }

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return TypedResults.BadRequest<object>(new { message = $"Invalid file type. Allowed types: {string.Join(", ", AllowedExtensions)}" });
        }

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{extension}";
        var uploadPath = Path.Combine(env.WebRootPath, "images", "menu-items");
        var filePath = Path.Combine(uploadPath, fileName);

        // Ensure directory exists
        Directory.CreateDirectory(uploadPath);

        // Delete old image if exists
        if (!string.IsNullOrEmpty(menuItem.ImageUrl))
        {
            var oldFileName = Path.GetFileName(menuItem.ImageUrl);
            var oldFilePath = Path.Combine(uploadPath, oldFileName);
            if (File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }
        }

        // Save new image
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Update database
        menuItem.ImageUrl = $"/images/menu-items/{fileName}";
        await db.SaveChangesAsync();

        return TypedResults.Ok<object>(new { imageUrl = menuItem.ImageUrl });
    }

    // private static async Task<Ok<List<string>>> GetMenuItemSuggestions(
    //     string? search,
    //     MenuDbContext db)
    // {
    //     if (string.IsNullOrWhiteSpace(search))
    //         return TypedResults.Ok(new List<string>());

    //     var suggestions = await db.MenuItems
    //         .Where(m => m.Name.Contains(search))
    //         .OrderBy(m => m.Name)
    //         .Select(m => m.Name)
    //         .Distinct()
    //         .Take(10)
    //         .ToListAsync();

    //     return TypedResults.Ok(suggestions);
    // }
}
