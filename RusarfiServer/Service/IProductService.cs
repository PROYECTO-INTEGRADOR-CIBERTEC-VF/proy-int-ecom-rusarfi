using RusarfiServer.Dtos.Products;

namespace RusarfiServer.Service;

public interface IProductService
{
    Task<ServiceResult<List<ProductDto>>> GetAvailableProductsAsync(string? search, string? category, CancellationToken cancellationToken);
}