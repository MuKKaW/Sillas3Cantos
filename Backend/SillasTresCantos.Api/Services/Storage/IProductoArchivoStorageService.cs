using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services.Storage;

public interface IProductoArchivoStorageService
{
    Task<StoredProductoArchivo> UploadAsync(IFormFile archivo, int productoId, CancellationToken cancellationToken);
    Task<ProductoArchivoDownload> GetDownloadAsync(ProductoArchivo archivo, CancellationToken cancellationToken);
    Task DeleteAsync(ProductoArchivo archivo, CancellationToken cancellationToken);
}

public sealed record StoredProductoArchivo(
    string NombreAlmacenado,
    string RutaRelativa,
    string? Url,
    string? PublicId,
    string? ResourceType,
    string Provider);

public sealed record ProductoArchivoDownload(
    string? AbsoluteFilePath,
    string? RedirectUrl,
    string ContentType,
    string FileName);
