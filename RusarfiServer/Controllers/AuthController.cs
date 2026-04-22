using Microsoft.AspNetCore.Mvc;
using RusarfiServer.Dtos.Auth;
using RusarfiServer.Dtos.Common;
using RusarfiServer.Service;

namespace RusarfiServer.Controllers;


//COMENTARIO EJEMPLO 
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return StatusCode(result.StatusCode, ApiResponse<object>.Ok(result.Message, result.Data));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, cancellationToken);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.Message));
        }

        return Ok(ApiResponse<object>.Ok(result.Message, result.Data));
    }
}
