using Microsoft.EntityFrameworkCore;
using RusarfiServer.Data;
using RusarfiServer.Dtos.Categories;

namespace RusarfiServer.Service;

public sealed class CategoryService(AppDbContext db) : ICategoryService
{
    public async Task<ServiceResult<List<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto { Id = c.Id, Name = c.Name })
            .ToListAsync(cancellationToken);

        return ServiceResult<List<CategoryDto>>.Ok("Categorías obtenidas correctamente", categories, 200);
    }
}
