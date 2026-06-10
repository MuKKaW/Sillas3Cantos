namespace SillasTresCantos.Api.Services.Storage;

public interface IProductoImagenStorageService
{
    Task<StoredProductoImagen> UploadAsync(IFormFile imagen, CancellationToken cancellationToken = default);
    Task DeleteAsync(string? publicId, CancellationToken cancellationToken = default);
}

public sealed record StoredProductoImagen(
    string Url,
    string PublicId,
    string ResourceType);
