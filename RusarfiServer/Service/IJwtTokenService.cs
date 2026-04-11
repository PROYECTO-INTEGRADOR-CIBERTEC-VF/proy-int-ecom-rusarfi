using RusarfiServer.Models;

namespace RusarfiServer.Service;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
}
