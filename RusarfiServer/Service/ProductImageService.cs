using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace RusarfiServer.Service;

public sealed class ProductImageService(IWebHostEnvironment environment) : IProductImageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private readonly string imagesRoot = BuildImagesRoot(environment);

    public async Task<string> SaveProductImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException("Archivo invalido.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Formato de imagen no soportado.");
        }

        var baseName = Path.GetFileNameWithoutExtension(file.FileName);
        baseName = Regex.Replace(baseName, "[^a-zA-Z0-9_-]", "-").Trim('-');
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "producto";
        }

        var fileName = $"{baseName}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(imagesRoot, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/images/productos/{fileName}";
    }

    private static string BuildImagesRoot(IWebHostEnvironment environment)
    {
        var webRoot = environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        var imagesRoot = Path.Combine(webRoot, "images", "productos");
        Directory.CreateDirectory(imagesRoot);

        return imagesRoot;
    }
}
