using RusarfiServer.Dtos.Cart;

namespace RusarfiServer.Service;

public interface ICartService
{
    Task<ServiceResult<CartSummaryDto>> AddProductAsync(CartAddRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<CartSummaryDto>> GetCartAsync(int userId, CancellationToken cancellationToken);
    Task<ServiceResult<CartSummaryDto>> UpdateQuantityAsync(CartUpdateRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<CartSummaryDto>> RemoveProductAsync(CartRemoveRequest request, CancellationToken cancellationToken);
}