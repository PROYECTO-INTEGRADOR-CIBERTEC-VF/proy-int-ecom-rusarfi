namespace RusarfiServer.Dtos.Orders;

public sealed class OrderDto
{
    public int OrderId { get; init; }
    public int UserId { get; init; }
    public decimal Total { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
}