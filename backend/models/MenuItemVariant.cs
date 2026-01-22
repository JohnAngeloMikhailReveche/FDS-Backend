using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KapeBara.MenuService.Models;

public class MenuItemVariant
{
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    // Foreign keys
    public int MenuItemId { get; set; }
    public int VariantId { get; set; }

    // Navigation properties
    public MenuItem MenuItem { get; set; } = null!;
    public Variant Variant { get; set; } = null!;
}
