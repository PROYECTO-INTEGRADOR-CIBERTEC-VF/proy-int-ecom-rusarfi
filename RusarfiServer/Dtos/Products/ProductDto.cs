namespace RusarfiServer.Dtos.Products;

public sealed class ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string ImageUrl { get; init; } = string.Empty;
    public int Stock { get; init; }
    public bool IsActive { get; init; }
}