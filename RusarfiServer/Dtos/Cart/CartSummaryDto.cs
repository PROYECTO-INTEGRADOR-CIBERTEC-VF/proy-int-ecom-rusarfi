namespace RusarfiServer.Dtos.Cart;

public sealed class CartSummaryDto
{
    public int UserId { get; init; }
    public List<CartItemDto> Items { get; init; } = new();
    public decimal Total { get; init; }
}