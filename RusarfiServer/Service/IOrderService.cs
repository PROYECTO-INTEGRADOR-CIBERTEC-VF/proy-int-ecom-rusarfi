using RusarfiServer.Dtos.Orders;

namespace RusarfiServer.Service;

public interface IOrderService
{
    Task<ServiceResult<ConfirmOrderResponse>> ConfirmOrderAsync(ConfirmOrderRequest request, CancellationToken cancellationToken);

    Task<ServiceResult<List<OrderDto>>> GetOrdersByUserAsync(
        int userId,
        string? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken);

    Task<ServiceResult<OrderDto>> GetOrderByIdAsync(
        int userId,
        int orderId,
        CancellationToken cancellationToken);
}