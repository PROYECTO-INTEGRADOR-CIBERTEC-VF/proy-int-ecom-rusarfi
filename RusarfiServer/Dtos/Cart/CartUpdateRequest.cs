using System.ComponentModel.DataAnnotations;

namespace RusarfiServer.Dtos.Cart;

public sealed class CartUpdateRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio")]
    public int UserId { get; init; }

    [Required(ErrorMessage = "El producto es obligatorio")]
    public int ProductId { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa")]
    public int Quantity { get; init; }
}