using System.ComponentModel.DataAnnotations;

namespace KapeBara.MenuService.Models;

public class MenuItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Foreign key
    public int CategoryId { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
    public ICollection<MenuItemVariant> MenuItemVariants { get; set; } = new List<MenuItemVariant>();
}
