using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using RusarfiServer.Data;
using RusarfiServer.Dtos.Auth;
using RusarfiServer.Dtos.Users;
using RusarfiServer.Models;

namespace RusarfiServer.Service;

public sealed class AuthService(AppDbContext db, IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<ServiceResult<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var name = (request.Name ?? string.Empty).Trim();
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult<UserDto>.Fail("El nombre es obligatorio", 400);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return ServiceResult<UserDto>.Fail("El correo es obligatorio", 400);
        }

        var emailExists = await db.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (emailExists)
        {
            return ServiceResult<UserDto>.Fail("El correo ya está registrado", 409);
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAtUtc = DateTime.UtcNow,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        var dto = new UserDto { Id = user.Id, Name = user.Name, Email = user.Email };
        return ServiceResult<UserDto>.Ok("Usuario registrado correctamente", dto, 201);
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = (request.Email ?? string.Empty).Trim().ToLowerInvariant();
        var password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return ServiceResult<AuthResponse>.Fail("El correo y la contraseña son obligatorios", 400);
        }

        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user is null)
        {
            return ServiceResult<AuthResponse>.Fail("Correo o contraseña inválidos", 401);
        }

        var ok = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!ok)
        {
            return ServiceResult<AuthResponse>.Fail("Correo o contraseña inválidos", 401);
        }

        var (token, expiresAtUtc) = jwtTokenService.CreateToken(user);
        var response = new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = new UserDto { Id = user.Id, Name = user.Name, Email = user.Email }
        };

        return ServiceResult<AuthResponse>.Ok("Login exitoso", response, 200);
    }
}
