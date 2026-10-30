using Microsoft.Extensions.Configuration;
using Pump.Application.Interfaces.Common;


namespace Pump.Infrastructure.Services.Storage;
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IConfiguration config)
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
        _baseUrl = config["App:BaseUrl"] ?? "https://localhost:7068";
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var ext = contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new ArgumentException("Only JPEG, PNG, and WebP images are supported.", nameof(contentType))
        };
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(_basePath, uniqueName);

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output, ct);

        return $"{_baseUrl}/avatars/{uniqueName}";
    }

    public async Task<string> SaveProtocolImageAsync(Stream fileStream, string contentType, CancellationToken ct = default)
    {
        var extension = contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new ArgumentException("Only JPEG, PNG, and WebP images are supported.", nameof(contentType))
        };

        var directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "protocols");
        Directory.CreateDirectory(directory);

        var uniqueName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(directory, uniqueName);

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output, ct);

        return $"{_baseUrl}/protocols/{uniqueName}";
    }

    public async Task<string> SaveMachineImageAsync(Stream fileStream, string contentType, CancellationToken ct = default)
    {
        var extension = GetImageExtension(contentType);
        var directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "machines");
        Directory.CreateDirectory(directory);

        var uniqueName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(directory, uniqueName);

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output, ct);

        return $"{_baseUrl}/machines/{uniqueName}";
    }

    private static string GetImageExtension(string contentType)
        => contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new ArgumentException("Only JPEG, PNG, and WebP images are supported.", nameof(contentType))
        };
}
