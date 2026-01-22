using System.ComponentModel.DataAnnotations;

namespace KapeBara.MenuService.Dtos.Variants;

public record CreateVariantRequest(
    [Required]
    [MaxLength(50)]
    string Name
);
