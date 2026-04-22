using Microsoft.EntityFrameworkCore;
using RusarfiServer.Data;
using RusarfiServer.Dtos.Products;

namespace RusarfiServer.Service;

public sealed class ProductService(AppDbContext db) : IProductService
{
    public async Task<ServiceResult<List<ProductDto>>> GetAvailableProductsAsync(string? search, string? category, CancellationToken cancellationToken)
    {
        var normalizedSearch = (search ?? string.Empty).Trim().ToLower();
        var normalizedCategory = (category ?? string.Empty).Trim().ToLower();

        var query = db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.Stock > 0);

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(p => p.Name.ToLower().Contains(normalizedSearch));
        }

        if (!string.IsNullOrWhiteSpace(normalizedCategory))
        {
            query = query.Where(p => p.Category.Name.ToLower().Contains(normalizedCategory));
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category.Name,
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
}