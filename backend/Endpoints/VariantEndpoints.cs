using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using KapeBara.MenuService.Data;
using KapeBara.MenuService.Dtos.Variants;
using KapeBara.MenuService.Models;

namespace KapeBara.MenuService.Endpoints;

public static class VariantEndpoints
{
    public static RouteGroupBuilder MapVariantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/variants")
            .WithTags("Variants");

        // GET: /api/variants
        group.MapGet("/", GetAllVariants)
            .WithName("GetAllVariants")
            .WithSummary("Get all variants")
            .WithDescription("Retrieves all variants (e.g., Solo, M, L, 3pcs) ordered by name.");

        // GET: /api/variants/{id}
        group.MapGet("/{id:int}", GetVariantById)
            .WithName("GetVariantById")
            .WithSummary("Get variant by ID")
            .WithDescription("Retrieves a specific variant by its ID.");

        // POST: /api/variants
        group.MapPost("/", CreateVariant)
            .WithName("CreateVariant")
            .WithSummary("Create a new variant")
            .WithDescription("Creates a new variant (e.g., Small, Medium, Large, Solo, 3pcs).");

        // PUT: /api/variants/{id}
        group.MapPut("/{id:int}", UpdateVariant)
            .WithName("UpdateVariant")
            .WithSummary("Update a variant")
            .WithDescription("Updates an existing variant by its ID.");

        // DELETE: /api/variants/{id}
        group.MapDelete("/{id:int}", DeleteVariant)
            .WithName("DeleteVariant")
            .WithSummary("Delete a variant")
            .WithDescription("Deletes a variant. This will also remove all menu item associations with this variant.");

        return group;
    }

    private static async Task<Ok<List<VariantResponse>>> GetAllVariants(MenuDbContext db)
    {
        var variants = await db.Variants
            .OrderBy(v => v.Name)
            .Select(v => new VariantResponse(
                v.Id,
                v.Name
            ))
            .ToListAsync();

        return TypedResults.Ok(variants);
    }

    private static async Task<Results<Ok<VariantResponse>, NotFound<object>>> GetVariantById(int id, MenuDbContext db)
    {
        var variant = await db.Variants
            .Where(v => v.Id == id)
            .Select(v => new VariantResponse(
                v.Id,
                v.Name
            ))
            .FirstOrDefaultAsync();

        if (variant is null)
        {
            return TypedResults.NotFound<object>(new { message = $"Variant with ID {id} not found." });
        }

        return TypedResults.Ok(variant);
    }

    private static async Task<Created<VariantResponse>> CreateVariant(CreateVariantRequest request, MenuDbContext db)
    {
        var variant = new Variant
        {
            Name = request.Name
        };

        db.Variants.Add(variant);
        await db.SaveChangesAsync();

        var response = new VariantResponse(
            variant.Id,
            variant.Name
        );

        return TypedResults.Created($"/api/variants/{variant.Id}", response);
    }

    private static async Task<Results<Ok<VariantResponse>, NotFound<object>>> UpdateVariant(int id, UpdateVariantRequest request, MenuDbContext db)
    {
        var variant = await db.Variants.FindAsync(id);

        if (variant is null)
        {
            return TypedResults.NotFound<object>(new { message = $"Variant with ID {id} not found." });
        }

        variant.Name = request.Name;

        await db.SaveChangesAsync();

        var response = new VariantResponse(
            variant.Id,
            variant.Name
        );

        return TypedResults.Ok(response);
    }

    private static async Task<Results<NoContent, NotFound<object>>> DeleteVariant(int id, MenuDbContext db)
    {
        var variant = await db.Variants.FindAsync(id);

        if (variant is null)
        {
            return TypedResults.NotFound<object>(new { message = $"Variant with ID {id} not found." });
        }

        db.Variants.Remove(variant);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
