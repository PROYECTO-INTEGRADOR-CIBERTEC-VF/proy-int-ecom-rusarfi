using RusarfiServer.Dtos.Auth;
using RusarfiServer.Dtos.Users;

namespace RusarfiServer.Service;

public interface IAuthService
{
    Task<ServiceResult<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
