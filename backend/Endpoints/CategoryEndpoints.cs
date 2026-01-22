using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using KapeBara.MenuService.Data;
using KapeBara.MenuService.Dtos.Categories;
using KapeBara.MenuService.Models;

namespace KapeBara.MenuService.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories");

        // GET: /api/categories
        group.MapGet("/", GetAllCategories)
            .WithName("GetAllCategories")
            .WithSummary("Get all categories")
            .WithDescription("Retrieves all categories ordered by name.");

        // GET: /api/categories/{id}
        group.MapGet("/{id:int}", GetCategoryById)
            .WithName("GetCategoryById")
            .WithSummary("Get category by ID")
            .WithDescription("Retrieves a specific category by its ID.");

        // POST: /api/categories
        // group.MapPost("/", CreateCategory)
        //     .WithName("CreateCategory")
        //     .WithSummary("Create a new category")
        //     .WithDescription("Creates a new category in the menu.");

        // PUT: /api/categories/{id}
        // group.MapPut("/{id:int}", UpdateCategory)
        //     .WithName("UpdateCategory")
        //     .WithSummary("Update a category")
        //     .WithDescription("Updates an existing category by its ID.");

        // DELETE: /api/categories/{id}
        // group.MapDelete("/{id:int}", DeleteCategory)
        //     .WithName("DeleteCategory")
        //     .WithSummary("Delete a category")
        //     .WithDescription("Deletes a category and all its associated menu items.");

        return group;
    }

    private static async Task<Ok<List<CategoryResponse>>> GetAllCategories(MenuDbContext db)
    {
        var categories = await db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryResponse(
                c.Id,
                c.Name
            ))
            .ToListAsync();

        return TypedResults.Ok(categories);
    }

    private static async Task<Results<Ok<CategoryResponse>, NotFound<object>>> GetCategoryById(int id, MenuDbContext db)
    {
        var category = await db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponse(
                c.Id,
                c.Name
            ))
            .FirstOrDefaultAsync();

        if (category is null)
        {
            return TypedResults.NotFound<object>(new { message = $"Category with ID {id} not found." });
        }

        return TypedResults.Ok(category);
    }

    // private static async Task<Created<CategoryResponse>> CreateCategory(CreateCategoryRequest request, MenuDbContext db)
    // {
    //     var category = new Category
    //     {
    //         Name = request.Name
    //     };

    //     db.Categories.Add(category);
    //     await db.SaveChangesAsync();

    //     var response = new CategoryResponse(
    //         category.Id,
    //         category.Name
    //     );

    //     return TypedResults.Created($"/api/categories/{category.Id}", response);
    // }

    // private static async Task<Results<Ok<CategoryResponse>, NotFound<object>>> UpdateCategory(int id, UpdateCategoryRequest request, MenuDbContext db)
    // {
    //     var category = await db.Categories.FindAsync(id);

    //     if (category is null)
    //     {
    //         return TypedResults.NotFound<object>(new { message = $"Category with ID {id} not found." });
    //     }

    //     category.Name = request.Name;

    //     await db.SaveChangesAsync();

    //     var response = new CategoryResponse(
    //         category.Id,
    //         category.Name
    //     );

    //     return TypedResults.Ok(response);
    // }

    // private static async Task<Results<NoContent, NotFound<object>>> DeleteCategory(int id, MenuDbContext db)
    // {
    //     var category = await db.Categories.FindAsync(id);

    //     if (category is null)
    //     {
    //         return TypedResults.NotFound<object>(new { message = $"Category with ID {id} not found." });
    //     }

    //     db.Categories.Remove(category);
    //     await db.SaveChangesAsync();

    //     return TypedResults.NoContent();
    // }
}
