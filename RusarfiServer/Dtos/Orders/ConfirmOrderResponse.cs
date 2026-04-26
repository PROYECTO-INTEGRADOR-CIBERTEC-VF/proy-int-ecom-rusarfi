namespace RusarfiServer.Dtos.Orders;

public sealed class ConfirmOrderResponse
{
    public int OrderId { get; init; }
    public string OrderNumber => $"PED-{OrderId:D4}";
    public decimal Total { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
}