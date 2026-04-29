using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RusarfiServer.Data;
using RusarfiServer.Dtos.Common;
using RusarfiServer.Dtos.Users;

namespace RusarfiServer.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly AppDbContext db;

    public UsersController(AppDbContext db) => this.db = db;

    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetUser(int userId, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserDto { Id = u.Id, Name = u.Name, Email = u.Email })
            .SingleOrDefaultAsync(cancellationToken);

        if (user == null)
            return NotFound(ApiResponse<object>.Fail("Usuario no encontrado"));

        return Ok(ApiResponse<UserDto>.Ok("Usuario obtenido correctamente", user));
    }
}
