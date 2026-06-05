using Application.Abstractions;
using Application.Exceptions;
using Application.Storage;
using Microsoft.Extensions.Options;

namespace Infrastructure.FileStorage;

public class LocalFileStorage : IFileStorage
{
    private readonly FileStorageOptions _options;
    private readonly string rootPath;

    public LocalFileStorage(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;
        rootPath = _options.RootPath;
    }

    public Task<Stream> OpenReadAsync(string storedFileName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var safeFileName = Path.GetFileName(storedFileName);
        var fullPath = Path.Combine(rootPath, safeFileName);

        if (!File.Exists(fullPath))
        {
            throw new AppException(ErrorCode.FileNotFound);
        }

        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken)
    {
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(rootPath, storedFileName);

        Directory.CreateDirectory(rootPath);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return storedFileName;
    }
    
    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var safeFileName = Path.GetFileName(storedFileName);
        var fullPath = Path.Combine(rootPath, safeFileName);
        
        File.Delete(fullPath);

        return Task.CompletedTask;
    }
}