using Microsoft.EntityFrameworkCore;
using RusarfiServer.Data;
using RusarfiServer.Dtos.Products;

namespace RusarfiServer.Service;

public sealed class ProductService(AppDbContext db) : IProductService
{
    public async Task<ServiceResult<List<ProductDto>>> GetAvailableProductsAsync(string? search, string? category, CancellationToken cancellationToken)
    {
        var normalizedSearch = Normalize(search);
        var normalizedCategory = Normalize(category);

        var query = db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.Stock > 0);

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(p => p.Name.ToLower().Contains(normalizedSearch));
        }

        if (!string.IsNullOrWhiteSpace(normalizedCategory))
        {
            query = query.Where(p => p.Category.ToLower().Contains(normalizedCategory));
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync(cancellationToken);

        if (products.Count == 0)
        {
            return ServiceResult<List<ProductDto>>.Ok("No hay productos disponibles", products, 200);
        }

        return ServiceResult<List<ProductDto>>.Ok("Productos obtenidos correctamente", products, 200);
    }

    public async Task<ServiceResult<ProductDetailDto>> GetProductDetailAsync(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return ServiceResult<ProductDetailDto>.Fail("Producto no encontrado", 404);
        }

        if (!product.IsActive)
        {
            return ServiceResult<ProductDetailDto>.Fail("El producto no está disponible o fue eliminado", 404);
        }

        var detail = new ProductDetailDto
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category,
            Description = product.Description,
            Price = product.Price,
            ImageUrl = product.ImageUrl,
            Stock = product.Stock
        };

        return ServiceResult<ProductDetailDto>.Ok("Detalle del producto obtenido correctamente", detail, 200);
    }

    public async Task<ServiceResult<List<ProductDto>>> GetRelatedProductsAsync(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null || !product.IsActive)
        {
            return ServiceResult<List<ProductDto>>.Fail("Producto no encontrado", 404);
        }

        var relatedProducts = await db.Products
            .AsNoTracking()
            .Where(p =>
                p.Id != id &&
                p.IsActive &&
                p.Stock > 0 &&
                p.Category.ToLower() == product.Category.ToLower())
            .OrderBy(p => p.Name)
            .Take(4)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price,
                ImageUrl = p.ImageUrl
            })
            .ToListAsync(cancellationToken);

        return ServiceResult<List<ProductDto>>.Ok("Productos relacionados obtenidos correctamente", relatedProducts, 200);
    }

    private static string Normalize(string? value)
        => (value ?? string.Empty).Trim().ToLower();
}