using System.ComponentModel.DataAnnotations;

namespace KapeBara.MenuService.Models;

public class Variant
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    // Navigation property
    public ICollection<MenuItemVariant> MenuItemVariants { get; set; } = new List<MenuItemVariant>();
}
