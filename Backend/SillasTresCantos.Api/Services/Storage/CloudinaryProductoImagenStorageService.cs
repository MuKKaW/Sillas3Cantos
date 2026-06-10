using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SillasTresCantos.Api.Services.Storage;

public sealed class CloudinaryProductoImagenStorageService : IProductoImagenStorageService
{
    private const string ProductosFolder = "sillas3cantos/productos";
    private readonly CloudinaryWrapper _cloudinaryWrapper;

    public CloudinaryProductoImagenStorageService(CloudinaryWrapper cloudinaryWrapper)
    {
        _cloudinaryWrapper = cloudinaryWrapper;
    }

    public async Task<StoredProductoImagen> UploadAsync(IFormFile imagen, CancellationToken cancellationToken = default)
    {
        Cloudinary cloudinary = _cloudinaryWrapper.GetRequiredClient();

        await using Stream stream = imagen.OpenReadStream();
        ImageUploadParams uploadParams = new()
        {
            File = new FileDescription(imagen.FileName, stream),
            Folder = ProductosFolder,
            Transformation = new Transformation().Width(1200).Crop("limit")
        };

        ImageUploadResult result = await cloudinary.UploadAsync(uploadParams, cancellationToken);
        if (result.Error is not null)
        {
            throw new InvalidOperationException($"No se pudo subir la imagen a Cloudinary: {result.Error.Message}");
        }

        string? url = result.SecureUrl?.ToString();
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(result.PublicId))
        {
            throw new InvalidOperationException("Cloudinary no devolvio los metadatos de la imagen subida.");
        }

        string resourceType = string.IsNullOrWhiteSpace(result.ResourceType)
            ? "image"
            : result.ResourceType;

        return new StoredProductoImagen(url, result.PublicId, resourceType);
    }

    public async Task DeleteAsync(string? publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        Cloudinary cloudinary = _cloudinaryWrapper.GetRequiredClient();
        DeletionParams deletionParams = new(publicId)
        {
            ResourceType = ResourceType.Image
        };

        DeletionResult result = await cloudinary.DestroyAsync(deletionParams);
        if (!string.Equals(result.Result, "ok", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(result.Result, "not found", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"No se pudo borrar la imagen de Cloudinary: {result.Result}");
        }
    }
}
