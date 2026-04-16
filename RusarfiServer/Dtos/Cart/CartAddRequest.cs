using System.ComponentModel.DataAnnotations;

namespace RusarfiServer.Dtos.Cart;

public sealed class CartAddRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio")]
    public int UserId { get; init; }

    [Required(ErrorMessage = "El producto es obligatorio")]
    public int ProductId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Quantity { get; init; } = 1;
}