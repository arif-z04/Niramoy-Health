namespace NiramoyHealth.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream stream, string filename, string folder, CancellationToken ct = default);
    Task<Stream?> GetFileAsync(string fileKey, CancellationToken ct = default);
    Task DeleteFileAsync(string fileKey, CancellationToken ct = default);
}
