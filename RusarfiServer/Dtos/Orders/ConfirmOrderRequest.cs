using System.ComponentModel.DataAnnotations;

namespace RusarfiServer.Dtos.Orders;

public sealed class ConfirmOrderRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio")]
    public int UserId { get; init; }
}