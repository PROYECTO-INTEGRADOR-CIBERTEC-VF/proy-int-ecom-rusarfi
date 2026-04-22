using Microsoft.AspNetCore.Mvc;
using RusarfiServer.Dtos.Common;
using RusarfiServer.Service;

namespace RusarfiServer.Controllers;

[ApiController]
[Route("api/admin/categories")]
public sealed class AdminCategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await categoryService.GetCategoriesAsync(cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }
}
