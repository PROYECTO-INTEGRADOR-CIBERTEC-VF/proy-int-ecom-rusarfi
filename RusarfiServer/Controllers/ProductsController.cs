using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RusarfiServer.Dtos.Common;
using RusarfiServer.Dtos.Products;
using RusarfiServer.Service;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RusarfiServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(
    IProductService productService,
    IProductImageService productImageService) : ControllerBase
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
    public async Task<IActionResult> GetProductDetail(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await productService.GetProductDetailAsync(id, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }

    [HttpGet("{id:int}/related")]
    public async Task<IActionResult> GetRelatedProducts(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await productService.GetRelatedProductsAsync(id, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }

    [HttpPost("image")]
    public async Task<IActionResult> UploadProductImage(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("Archivo invalido."));
        }

        try
        {
            var imageUrl = await productImageService.SaveProductImageAsync(file, cancellationToken);
            var response = new ProductImageResponse { ImageUrl = imageUrl };

            return Ok(ApiResponse<ProductImageResponse>.Ok("Imagen cargada correctamente", response));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
        catch
        {
            return StatusCode(500, ApiResponse<object>.Fail("No se pudo guardar la imagen."));
        }
    }
}