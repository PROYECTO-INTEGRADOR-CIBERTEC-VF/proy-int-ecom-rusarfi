using System.ComponentModel.DataAnnotations;

namespace RusarfiServer.Dtos.Products;

public sealed class CreateProductRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "La categoría es obligatoria")]
    public int CategoryId { get; init; }

    [MaxLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string Description { get; init; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Price { get; init; }

    [MaxLength(500, ErrorMessage = "La URL de imagen no puede exceder 500 caracteres")]
    public string ImageUrl { get; init; } = string.Empty;

    [Required(ErrorMessage = "El stock es obligatorio")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    public int Stock { get; init; }
}