using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using KapeBara.MenuService.Data;
using KapeBara.MenuService.Dtos.MenuItemVariants;
using KapeBara.MenuService.Models;

namespace KapeBara.MenuService.Endpoints;

public static class MenuItemVariantEndpoints
{
    public static RouteGroupBuilder MapMenuItemVariantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/menu-items/{menuItemId:int}/variants")
            .WithTags("Menu Item Variants");

        // GET: /api/menu-items/{menuItemId}/variants
        group.MapGet("/", GetMenuItemVariants)
            .WithName("GetMenuItemVariants")
            .WithSummary("Get all variants for a menu item")
            .WithDescription("Retrieves all variants with prices for a specific menu item.");

        // POST: /api/menu-items/{menuItemId}/variants
        group.MapPost("/", CreateMenuItemVariant)
            .WithName("CreateMenuItemVariant")
            .WithSummary("Add a variant to a menu item")
            .WithDescription("Assigns a variant (with price) to a menu item.");

        // PUT: /api/menu-items/{menuItemId}/variants/{id}
        group.MapPut("/{id:int}", UpdateMenuItemVariant)
            .WithName("UpdateMenuItemVariant")
            .WithSummary("Update menu item variant price")
            .WithDescription("Updates the price of a variant for a specific menu item.");

        // DELETE: /api/menu-items/{menuItemId}/variants/{id}
        group.MapDelete("/{id:int}", DeleteMenuItemVariant)
            .WithName("DeleteMenuItemVariant")
            .WithSummary("Remove a variant from a menu item")
            .WithDescription("Removes a variant assignment from a menu item.");

        return group;
    }

    private static async Task<Results<Ok<List<MenuItemVariantResponse>>, NotFound<object>>> GetMenuItemVariants(int menuItemId, MenuDbContext db)
    {
        var menuItemExists = await db.MenuItems.AnyAsync(m => m.Id == menuItemId);
        if (!menuItemExists)
        {
            return TypedResults.NotFound<object>(new { message = $"Menu item with ID {menuItemId} not found." });
        }

        var variants = await db.MenuItemVariants
            .Include(miv => miv.Variant)
            .Where(miv => miv.MenuItemId == menuItemId)
            .OrderBy(miv => miv.Variant.Name)
            .Select(miv => new MenuItemVariantResponse(
                miv.Id,
                miv.VariantId,
                miv.Variant.Name,
                miv.Price
            ))
            .ToListAsync();

        return TypedResults.Ok(variants);
    }

    private static async Task<Results<Created<MenuItemVariantResponse>, NotFound<object>, BadRequest<object>, Conflict<object>>> CreateMenuItemVariant(
        int menuItemId,
        CreateMenuItemVariantRequest request,
        MenuDbContext db)
    {
        var menuItemExists = await db.MenuItems.AnyAsync(m => m.Id == menuItemId);
        if (!menuItemExists)
        {
            return TypedResults.NotFound<object>(new { message = $"Menu item with ID {menuItemId} not found." });
        }

        var variant = await db.Variants.FindAsync(request.VariantId);
        if (variant is null)
        {
            return TypedResults.BadRequest<object>(new { message = $"Variant with ID {request.VariantId} not found." });
        }

        // Check if this variant already exists for this menu item
        var existingVariant = await db.MenuItemVariants
            .AnyAsync(miv => miv.MenuItemId == menuItemId && miv.VariantId == request.VariantId);

        if (existingVariant)
        {
            return TypedResults.Conflict<object>(new { message = $"This menu item already has variant '{variant.Name}' assigned." });
        }

        var menuItemVariant = new MenuItemVariant
        {
            MenuItemId = menuItemId,
            VariantId = request.VariantId,
            Price = request.Price
        };

        db.MenuItemVariants.Add(menuItemVariant);
        await db.SaveChangesAsync();

        var response = new MenuItemVariantResponse(
            menuItemVariant.Id,
            menuItemVariant.VariantId,
            variant.Name,
            menuItemVariant.Price
        );

        return TypedResults.Created(
            $"/api/menu-items/{menuItemId}/variants/{menuItemVariant.Id}",
            response
        );
    }

    private static async Task<Results<Ok<MenuItemVariantResponse>, NotFound<object>>> UpdateMenuItemVariant(
        int menuItemId,
        int id,
        UpdateMenuItemVariantRequest request,
        MenuDbContext db)
    {
        var menuItemVariant = await db.MenuItemVariants
            .Include(miv => miv.Variant)
            .FirstOrDefaultAsync(miv => miv.Id == id && miv.MenuItemId == menuItemId);

        if (menuItemVariant is null)
        {
            return TypedResults.NotFound<object>(new { message = $"Menu item variant with ID {id} not found for menu item {menuItemId}." });
        }

        menuItemVariant.Price = request.Price;
        await db.SaveChangesAsync();

        var response = new MenuItemVariantResponse(
            menuItemVariant.Id,
            menuItemVariant.VariantId,
            menuItemVariant.Variant.Name,
            menuItemVariant.Price
        );

        return TypedResults.Ok(response);
    }

    private static async Task<Results<NoContent, NotFound<object>>> DeleteMenuItemVariant(int menuItemId, int id, MenuDbContext db)
    {
        var menuItemVariant = await db.MenuItemVariants
            .FirstOrDefaultAsync(miv => miv.Id == id && miv.MenuItemId == menuItemId);

        if (menuItemVariant is null)
        {
            return TypedResults.NotFound<object>(new { message = $"Menu item variant with ID {id} not found for menu item {menuItemId}." });
        }

        db.MenuItemVariants.Remove(menuItemVariant);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
