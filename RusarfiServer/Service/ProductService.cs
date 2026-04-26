using Microsoft.EntityFrameworkCore;
using RusarfiServer.Data;
using RusarfiServer.Dtos.Products;
using RusarfiServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace RusarfiServer.Service;

public sealed class ProductService(AppDbContext db) : IProductService
{
    public async Task<ServiceResult<List<ProductDto>>> GetAvailableProductsAsync(
        string? search,
        string? category,
        CancellationToken cancellationToken)
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
            .Select(ToDtoProjection)
            .ToListAsync(cancellationToken);

        if (products.Count == 0)
        {
            return ServiceResult<List<ProductDto>>.Ok("No hay productos disponibles", products, 200);
        }

        return ServiceResult<List<ProductDto>>.Ok("Productos obtenidos correctamente", products, 200);
    }

    public async Task<ServiceResult<ProductDetailDto>> GetProductDetailAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Stock = p.Stock
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return ServiceResult<ProductDetailDto>.Fail("Producto no encontrado", 404);
        }

        var isActive = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => p.IsActive)
            .SingleOrDefaultAsync(cancellationToken);

        if (!isActive)
        {
            return ServiceResult<ProductDetailDto>.Fail("El producto no está disponible o fue eliminado", 404);
        }

        return ServiceResult<ProductDetailDto>.Ok("Detalle del producto obtenido correctamente", product, 200);
    }

    public async Task<ServiceResult<List<ProductDto>>> GetRelatedProductsAsync(
        int id,
        CancellationToken cancellationToken)
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
                p.CategoryId == product.CategoryId)
            .OrderBy(p => p.Name)
            .Take(4)
            .Select(ToDtoProjection)
            .ToListAsync(cancellationToken);

        return ServiceResult<List<ProductDto>>.Ok("Productos relacionados obtenidos correctamente", relatedProducts, 200);
    }

    public async Task<ServiceResult<List<ProductDto>>> GetAllProductsAsync(
        string? search,
        string? category,
        CancellationToken cancellationToken)
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
            query = query.Where(p => p.Category.Name.ToLower().Contains(normalizedCategory));
        }

        var products = await query
            .OrderBy(p => p.Name)
            .Select(ToDtoProjection)
            .ToListAsync(cancellationToken);

        return ServiceResult<List<ProductDto>>.Ok("Productos obtenidos correctamente", products, 200);
    }

    public async Task<ServiceResult<ProductDto>> GetProductByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ToDtoProjection)
            .SingleOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return ServiceResult<ProductDto>.Fail("Producto no encontrado", 404);
        }

        return ServiceResult<ProductDto>.Ok("Producto obtenido correctamente", product, 200);
    }

    public async Task<ServiceResult<ProductDto>> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var category = await FindCategoryAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return ServiceResult<ProductDto>.Fail("Categoría no encontrada", 400);
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            CategoryId = category.Id,
            Category = category,
            Description = (request.Description ?? string.Empty).Trim(),
            Price = request.Price,
            ImageUrl = BuildImageUrl(request.ImageUrl),
            Stock = request.Stock,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<ProductDto>.Ok("Producto creado correctamente", ToDto(product), 201);
    }

    public async Task<ServiceResult<ProductDto>> UpdateProductAsync(
        int id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = await db.Products
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product is null)
        {
            return ServiceResult<ProductDto>.Fail("Producto no encontrado", 404);
        }

        var category = await FindCategoryAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return ServiceResult<ProductDto>.Fail("Categoría no encontrada", 400);
        }

        product.Name = request.Name.Trim();
        product.CategoryId = category.Id;
        product.Category = category;
        product.Description = (request.Description ?? string.Empty).Trim();
        product.Price = request.Price;
        product.ImageUrl = BuildImageUrl(request.ImageUrl);
        product.Stock = request.Stock;
        product.IsActive = request.IsActive;

        await db.SaveChangesAsync(cancellationToken);

        return ServiceResult<ProductDto>.Ok("Producto actualizado correctamente", ToDto(product), 200);
    }

    public async Task<ServiceResult<object>> DeleteProductAsync(
        int id,
        CancellationToken cancellationToken)
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

    private async Task<Category?> FindCategoryAsync(
        int categoryId,
        CancellationToken cancellationToken)
        => await db.Categories.SingleOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

    private static string BuildImageUrl(string? imageUrl)
    {
        var value = (imageUrl ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        if (value.StartsWith("/images/productos/", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        var fileName = Path.GetFileName(value);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        return $"/images/productos/{fileName}";
    }

    private static readonly Expression<Func<Product, ProductDto>> ToDtoProjection = p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        CategoryId = p.CategoryId,
        Category = p.Category.Name,
        Description = p.Description,
        Price = p.Price,
        ImageUrl = p.ImageUrl,
        Stock = p.Stock,
        IsActive = p.IsActive
    };

    private static ProductDto ToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        CategoryId = p.CategoryId,
        Category = p.Category?.Name ?? string.Empty,
        Description = p.Description,
        Price = p.Price,
        ImageUrl = p.ImageUrl,
        Stock = p.Stock,
        IsActive = p.IsActive
    };
}