using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RusarfiServer.Models;

namespace RusarfiServer.Service;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public (string Token, DateTime ExpiresAtUtc) CreateToken(User user)
    {
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var key = configuration["Jwt:Key"];
        var expiresMinutesRaw = configuration["Jwt:ExpiresMinutes"];

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("JWT configuration is missing (Jwt:Issuer, Jwt:Audience, Jwt:Key)");
        }

        var expiresMinutes = 60;
        _ = int.TryParse(expiresMinutesRaw, out expiresMinutes);
        if (expiresMinutes <= 0)
        {
            expiresMinutes = 60;
        }

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiresMinutes);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("name", user.Name),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expiresAtUtc);
    }
}
