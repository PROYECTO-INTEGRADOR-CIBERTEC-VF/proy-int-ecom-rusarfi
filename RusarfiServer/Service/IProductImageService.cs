using Microsoft.AspNetCore.Http;

namespace RusarfiServer.Service;

public interface IProductImageService
{
    Task<string> SaveProductImageAsync(IFormFile file, CancellationToken cancellationToken);
}
