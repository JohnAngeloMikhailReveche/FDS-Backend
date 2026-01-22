using System.ComponentModel.DataAnnotations;

namespace KapeBara.MenuService.Dtos.Variants;

public record UpdateVariantRequest(
    [Required]
    [MaxLength(50)]
    string Name
);
