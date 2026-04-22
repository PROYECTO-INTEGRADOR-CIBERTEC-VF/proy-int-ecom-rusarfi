using Microsoft.EntityFrameworkCore;
using RusarfiServer.Data;
using RusarfiServer.Dtos.Products;
using RusarfiServer.Models;

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
            query = query.Where(p => p.Category.Name.ToLower().Contains(normalizedCategory));
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Select(p => ToDto(p))
            .ToListAsync(cancellationToken);

        if (products.Count == 0)
        {
            return ServiceResult<List<ProductDto>>.Ok("No hay productos disponibles", products, 200);
        }

        return ServiceResult<List<ProductDto>>.Ok("Productos obtenidos correctamente", products, 200);
    }

    public async Task<ServiceResult<List<ProductDto>>> GetAllProductsAsync(string? search, string? category, CancellationToken cancellationToken)
    {
        var normalizedSearch = Normalize(search);
        var normalizedCategory = Normalize(category);

        var query = db.Products.AsNoTracking();

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
            .Select(p => ToDto(p))
            .ToListAsync(cancellationToken);

        return ServiceResult<List<ProductDto>>.Ok("Productos obtenidos correctamente", products, 200);
    }

    public async Task<ServiceResult<ProductDto>> GetProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return ServiceResult<ProductDto>.Fail("Producto no encontrado", 404);
        }

        return ServiceResult<ProductDto>.Ok("Producto obtenido correctamente", ToDto(product), 200);
    }

    public async Task<ServiceResult<ProductDto>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            Description = (request.Description ?? string.Empty).Trim(),
            Price = request.Price,
            ImageUrl = (request.ImageUrl ?? string.Empty).Trim(),
            Stock = request.Stock,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<ProductDto>.Ok("Producto creado correctamente", ToDto(product), 201);
    }

    public async Task<ServiceResult<ProductDto>> UpdateProductAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return ServiceResult<ProductDto>.Fail("Producto no encontrado", 404);
        }

        product.Name = request.Name.Trim();
        product.Category = request.Category.Trim();
        product.Description = (request.Description ?? string.Empty).Trim();
        product.Price = request.Price;
        product.ImageUrl = (request.ImageUrl ?? string.Empty).Trim();
        product.Stock = request.Stock;
        product.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<ProductDto>.Ok("Producto actualizado correctamente", ToDto(product), 200);
    }

    public async Task<ServiceResult<object>> DeleteProductAsync(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return ServiceResult<object>.Fail("Producto no encontrado", 404);
        }

        product.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<object>.Ok("Producto eliminado correctamente", null, 200);
    }

    private static string Normalize(string? value)
        => (value ?? string.Empty).Trim().ToLower();

    private static ProductDto ToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Category = p.Category,
        Description = p.Description,
        Price = p.Price,
        ImageUrl = p.ImageUrl,
        Stock = p.Stock,
        IsActive = p.IsActive
    };
}