using RusarfiServer.Dtos.Categories;

namespace RusarfiServer.Service;

public interface ICategoryService
{
    Task<ServiceResult<List<CategoryDto>>> GetCategoriesAsync(CancellationToken cancellationToken);
}
