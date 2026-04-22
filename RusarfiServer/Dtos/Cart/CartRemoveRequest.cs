using System.ComponentModel.DataAnnotations;

namespace RusarfiServer.Dtos.Cart;

public sealed class CartRemoveRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio")]
    public int UserId { get; init; }

    [Required(ErrorMessage = "El producto es obligatorio")]
    public int ProductId { get; init; }
}