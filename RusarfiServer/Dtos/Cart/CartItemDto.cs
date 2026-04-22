namespace RusarfiServer.Dtos.Cart;

public sealed class CartItemDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Subtotal { get; init; }
}