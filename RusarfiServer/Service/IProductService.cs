using RusarfiServer.Dtos.Products;

namespace RusarfiServer.Service;

public interface IProductService
{
    Task<ServiceResult<List<ProductDto>>> GetAvailableProductsAsync(string? search, string? category, CancellationToken cancellationToken);
    Task<ServiceResult<ProductDetailDto>> GetProductDetailAsync(int id, CancellationToken cancellationToken);
    Task<ServiceResult<List<ProductDto>>> GetRelatedProductsAsync(int id, CancellationToken cancellationToken);
}