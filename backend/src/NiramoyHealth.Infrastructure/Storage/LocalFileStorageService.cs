using Microsoft.Extensions.Configuration;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseStoragePath;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".pdf"
    };

    public LocalFileStorageService(IConfiguration config)
    {
        var configuredPath = config["Storage:LocalPath"] ?? "storage/uploads";
        _baseStoragePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(Directory.GetCurrentDirectory(), configuredPath);

        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream stream, string filename, string folder, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(filename).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "File", new[] { $"File type '{ext}' is not supported. Allowed extensions: .jpg, .jpeg, .png, .webp, .pdf" } }
            });
        }

        var folderPath = Path.Combine(_baseStoragePath, folder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
        var relativeKey = Path.Combine(folder, uniqueFileName).Replace('\\', '/');
        var fullPath = Path.Combine(_baseStoragePath, folder, uniqueFileName);

        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await stream.CopyToAsync(fileStream, ct);

        return relativeKey;
    }

    public Task<Stream?> GetFileAsync(string fileKey, CancellationToken ct = default)
    {
        var sanitizedKey = fileKey.Replace("..", "").TrimStart('/', '\\');
        var fullPath = Path.Combine(_baseStoragePath, sanitizedKey);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteFileAsync(string fileKey, CancellationToken ct = default)
    {
        var sanitizedKey = fileKey.Replace("..", "").TrimStart('/', '\\');
        var fullPath = Path.Combine(_baseStoragePath, sanitizedKey);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }
}
