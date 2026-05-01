using AiBoard.Application.Abstractions.AI;

namespace AiBoard.Infrastructure.AI;

public sealed class FileSystemGeneratedImageStore : IGeneratedImageStore
{
    public async Task<string> SaveBase64ImageAsync(string imageBase64, string mimeType, CancellationToken cancellationToken)
    {
        var extension = mimeType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/webp" => ".webp",
            _ => ".png"
        };

        var rootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "generated-images");
        Directory.CreateDirectory(rootPath);

        var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(rootPath, fileName);
        var bytes = Convert.FromBase64String(imageBase64);

        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);
        return $"/generated-images/{fileName}";
    }
}
