using RusarfiServer.Dtos.Cart;

namespace RusarfiServer.Service;

public interface ICartService
{
    Task<ServiceResult<CartSummaryDto>> AddProductAsync(CartAddRequest request, CancellationToken cancellationToken);
}