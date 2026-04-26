using Microsoft.AspNetCore.Mvc;
using RusarfiServer.Dtos.Cart;
using RusarfiServer.Dtos.Common;
using RusarfiServer.Service;

namespace RusarfiServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CartController(ICartService cartService) : ControllerBase
{
    [HttpPost("items")]
    public async Task<IActionResult> AddProduct([FromBody] CartAddRequest request, CancellationToken cancellationToken)
    {
        var result = await cartService.AddProductAsync(request, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetCart(int userId, CancellationToken cancellationToken)
    {
        var result = await cartService.GetCartAsync(userId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }

    [HttpPut("items")]
    public async Task<IActionResult> UpdateQuantity([FromBody] CartUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await cartService.UpdateQuantityAsync(request, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }

    [HttpDelete("items")]
    public async Task<IActionResult> RemoveProduct([FromBody] CartRemoveRequest request, CancellationToken cancellationToken)
    {
        var result = await cartService.RemoveProductAsync(request, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }
}