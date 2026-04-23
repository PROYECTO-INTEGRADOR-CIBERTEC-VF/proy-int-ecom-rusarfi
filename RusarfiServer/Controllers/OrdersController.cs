using Microsoft.AspNetCore.Mvc;
using RusarfiServer.Dtos.Common;
using RusarfiServer.Dtos.Orders;
using RusarfiServer.Service;

namespace RusarfiServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmOrder([FromBody] ConfirmOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await orderService.ConfirmOrderAsync(request, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetOrdersByUser(int userId, CancellationToken cancellationToken)
    {
        var result = await orderService.GetOrdersByUserAsync(userId, cancellationToken);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }
}