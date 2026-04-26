using RusarfiServer.Dtos.Products;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RusarfiServer.Service;

public interface IProductService
{
    Task<ServiceResult<List<ProductDto>>> GetAvailableProductsAsync(
        string? search,
        string? category,
        CancellationToken cancellationToken);

    Task<ServiceResult<ProductDetailDto>> GetProductDetailAsync(
        int id,
        CancellationToken cancellationToken);

    Task<ServiceResult<List<ProductDto>>> GetRelatedProductsAsync(
        int id,
        CancellationToken cancellationToken);

    Task<ServiceResult<List<ProductDto>>> GetAllProductsAsync(
        string? search,
        string? category,
        CancellationToken cancellationToken);

    Task<ServiceResult<ProductDto>> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<ServiceResult<ProductDto>> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken);

    Task<ServiceResult<ProductDto>> UpdateProductAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken);

    Task<ServiceResult<object>> DeleteProductAsync(
        int id,
        CancellationToken cancellationToken);
}