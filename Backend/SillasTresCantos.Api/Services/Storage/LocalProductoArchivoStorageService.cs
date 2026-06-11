using Microsoft.Extensions.Options;
using SillasTresCantos.Api.Configuration;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services.Storage;

public class LocalProductoArchivoStorageService : IProductoArchivoStorageService
{
    private readonly FileStorageOptions _fileStorageOptions;
    private readonly IWebHostEnvironment _environment;

    public LocalProductoArchivoStorageService(
        IOptions<FileStorageOptions> fileStorageOptions,
        IWebHostEnvironment environment)
    {
        _fileStorageOptions = fileStorageOptions.Value;
        _environment = environment;
    }

    public async Task<StoredProductoArchivo> UploadAsync(IFormFile archivo, int productoId, CancellationToken cancellationToken)
    {
        string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        string rootPath = GetStorageRootAbsolutePath();
        string productoPath = Path.Combine(rootPath, "productos", productoId.ToString());
        Directory.CreateDirectory(productoPath);

        string generatedFileName = $"{Guid.NewGuid():N}{extension}";
        string absolutePath = Path.Combine(productoPath, generatedFileName);

        try
        {
            await using FileStream outputStream = new(absolutePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await archivo.CopyToAsync(outputStream, cancellationToken);
        }
        catch
        {
            TryDeleteFile(absolutePath);
            throw;
        }

        string relativePath = Path.GetRelativePath(rootPath, absolutePath).Replace("\\", "/");

        return new StoredProductoArchivo(
            generatedFileName,
            relativePath,
            null,
            null,
            null,
            "local");
    }

    public Task<ProductoArchivoDownload> GetDownloadAsync(ProductoArchivo archivo, CancellationToken cancellationToken)
    {
        string contentType = string.IsNullOrWhiteSpace(archivo.ContentType)
            ? "application/octet-stream"
            : archivo.ContentType;

        if (!TryResolveAbsolutePath(archivo.RutaRelativa, out string absolutePath) || !File.Exists(absolutePath))
        {
            return Task.FromResult(new ProductoArchivoDownload(null, null, contentType, archivo.NombreOriginal));
        }

        return Task.FromResult(new ProductoArchivoDownload(absolutePath, null, contentType, archivo.NombreOriginal));
    }

    public Task DeleteAsync(ProductoArchivo archivo, CancellationToken cancellationToken)
    {
        if (TryResolveAbsolutePath(archivo.RutaRelativa, out string absolutePath))
        {
            TryDeleteFile(absolutePath);
        }

        return Task.CompletedTask;
    }

    private string GetStorageRootAbsolutePath()
    {
        string configuredPath = string.IsNullOrWhiteSpace(_fileStorageOptions.RootPath)
            ? "storage"
            : _fileStorageOptions.RootPath.Trim();

        if (Path.IsPathRooted(configuredPath))
        {
            return Path.GetFullPath(configuredPath);
        }

        return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, configuredPath));
    }

    private bool TryResolveAbsolutePath(string relativePath, out string absolutePath)
    {
        absolutePath = string.Empty;
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        string rootPath = GetStorageRootAbsolutePath();
        string rootFullPath = Path.GetFullPath(rootPath);
        string candidatePath = Path.GetFullPath(Path.Combine(rootFullPath, relativePath));
        string rootWithSeparator = rootFullPath.EndsWith(Path.DirectorySeparatorChar)
            ? rootFullPath
            : rootFullPath + Path.DirectorySeparatorChar;

        bool isInsideRoot = candidatePath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase);
        if (!isInsideRoot)
        {
            return false;
        }

        absolutePath = candidatePath;
        return true;
    }

    private static void TryDeleteFile(string? absolutePath)
    {
        if (string.IsNullOrWhiteSpace(absolutePath))
        {
            return;
        }

        try
        {
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
