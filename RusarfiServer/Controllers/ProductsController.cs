using Microsoft.AspNetCore.Mvc;
using RusarfiServer.Dtos.Common;
using RusarfiServer.Service;

namespace RusarfiServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAvailableProducts(
        [FromQuery] string? search,
        [FromQuery] string? category,
        CancellationToken cancellationToken)
    {
        var result = await productService.GetAvailableProductsAsync(search, category, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductDetail(int id, CancellationToken cancellationToken)
    {
        var result = await productService.GetProductDetailAsync(id, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }

    [HttpGet("{id:int}/related")]
    public async Task<IActionResult> GetRelatedProducts(int id, CancellationToken cancellationToken)
    {
        var result = await productService.GetRelatedProductsAsync(id, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }
}