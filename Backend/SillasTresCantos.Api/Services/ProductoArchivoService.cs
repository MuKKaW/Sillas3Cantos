using MySql.Data.MySqlClient;
using Microsoft.Extensions.Options;
using SillasTresCantos.Api.Configuration;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;
using SillasTresCantos.Api.Services.Storage;

namespace SillasTresCantos.Api.Services;

public class ProductoArchivoService : IProductoArchivoService
{
    private const long DefaultMaxFileSizeBytes = 5 * 1024 * 1024;
    private readonly IProductoArchivoRepository _productoArchivoRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly IProductoArchivoStorageService _storageService;
    private readonly FileStorageOptions _fileStorageOptions;
    private readonly HashSet<string> _allowedExtensions;

    public ProductoArchivoService(
        IProductoArchivoRepository productoArchivoRepository,
        IProductoRepository productoRepository,
        IOptions<FileStorageOptions> fileStorageOptions,
        IProductoArchivoStorageService storageService)
    {
        _productoArchivoRepository = productoArchivoRepository;
        _productoRepository = productoRepository;
        _storageService = storageService;
        _fileStorageOptions = fileStorageOptions.Value;
        _allowedExtensions = (_fileStorageOptions.AllowedExtensions ?? Array.Empty<string>())
            .Where(extension => !string.IsNullOrWhiteSpace(extension))
            .Select(extension => extension.Trim().ToLowerInvariant())
            .ToHashSet();
    }

    public async Task<ProductoArchivoOperationResult> GetArchivosAsync(int productoId, bool includeHidden, CancellationToken cancellationToken = default)
    {
        if (productoId <= 0)
        {
            return ProductoArchivoOperationResult.ValidationError();
        }

        bool canAccess;
        try
        {
            canAccess = await CanAccessProductoAsync(productoId, includeHidden, cancellationToken);
        }
        catch (MySqlException)
        {
            return ProductoArchivoOperationResult.UnexpectedError();
        }

        if (!canAccess)
        {
            return ProductoArchivoOperationResult.NotFoundError();
        }

        List<ProductoArchivo> archivos;
        try
        {
            archivos = await _productoArchivoRepository.GetArchivosByProductoIdAsync(productoId, cancellationToken);
        }
        catch (MySqlException)
        {
            return ProductoArchivoOperationResult.UnexpectedError();
        }

        List<GetProductoArchivoDTO> dtos = archivos
            .Select(MapToGetProductoArchivoDTO)
            .ToList();

        return ProductoArchivoOperationResult.SuccessArchivos(dtos);
    }

    public async Task<ProductoArchivoOperationResult> GetArchivoDescargaAsync(int productoId, int archivoId, bool includeHidden, CancellationToken cancellationToken = default)
    {
        if (productoId <= 0 || archivoId <= 0)
        {
            return ProductoArchivoOperationResult.ValidationError();
        }

        bool canAccess;
        try
        {
            canAccess = await CanAccessProductoAsync(productoId, includeHidden, cancellationToken);
        }
        catch (MySqlException)
        {
            return ProductoArchivoOperationResult.UnexpectedError();
        }

        if (!canAccess)
        {
            return ProductoArchivoOperationResult.NotFoundError();
        }

        ProductoArchivo? archivo;
        try
        {
            archivo = await _productoArchivoRepository.GetArchivoByIdAsync(archivoId, cancellationToken);
        }
        catch (MySqlException)
        {
            return ProductoArchivoOperationResult.UnexpectedError();
        }

        if (archivo is null || archivo.ProductoId != productoId)
        {
            return ProductoArchivoOperationResult.NotFoundError();
        }

        ProductoArchivoDownload download;
        try
        {
            download = await _storageService.GetDownloadAsync(archivo, cancellationToken);
        }
        catch (IOException)
        {
            return ProductoArchivoOperationResult.UnexpectedError();
        }
        catch (UnauthorizedAccessException)
        {
            return ProductoArchivoOperationResult.UnexpectedError();
        }

        if (string.IsNullOrWhiteSpace(download.AbsoluteFilePath))
        {
            return ProductoArchivoOperationResult.NotFoundError();
        }

        GetProductoArchivoDTO dto = MapToGetProductoArchivoDTO(archivo);

        return ProductoArchivoOperationResult.SuccessDescarga(
            dto,
            download.AbsoluteFilePath,
            download.ContentType,
            download.FileName);
    }

    public async Task<ProductoArchivoOperationResult> UploadArchivoAsync(int productoId, IFormFile? archivo, int subidoPorUsuarioId, CancellationToken cancellationToken = default)
    {
        if (productoId <= 0 || subidoPorUsuarioId <= 0 || archivo is null || archivo.Length <= 0)
        {
            return ProductoArchivoOperationResult.ValidationError();
        }

        long maxFileSize = _fileStorageOptions.MaxFileSizeBytes > 0
            ? _fileStorageOptions.MaxFileSizeBytes
            : DefaultMaxFileSizeBytes;

        if (archivo.Length > maxFileSize)
        {
            return ProductoArchivoOperationResult.FileTooLargeError();
        }

        string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        bool extensionAllowed = _allowedExtensions.Count == 0 || _allowedExtensions.Contains(extension);
        if (!extensionAllowed)
        {
            return ProductoArchivoOperationResult.UnsupportedTypeError();
        }

        Producto? producto;
        try
        {
            producto = await _productoRepository.GetProductoByIdAsync(productoId, cancellationToken);
        }
        catch (MySqlException)
        {
            return ProductoArchivoOperationResult.UnexpectedError();
        }

        if (producto is null)
        {
            return ProductoArchivoOperationResult.NotFoundError();
        }

        ProductoArchivo? nuevoArchivo = null;
        try
        {
            StoredProductoArchivo storedArchivo = await _storageService.UploadAsync(archivo, productoId, cancellationToken);
            string contentType = string.IsNullOrWhiteSpace(archivo.ContentType)
                ? "application/octet-stream"
                : archivo.ContentType;

            nuevoArchivo = new ProductoArchivo
            {
                Id = 0,
                ProductoId = productoId,
                SubidoPorUsuarioId = subidoPorUsuarioId,
                NombreOriginal = Path.GetFileName(archivo.FileName),
                NombreAlmacenado = storedArchivo.NombreAlmacenado,
                RutaRelativa = storedArchivo.RutaRelativa,
                ContentType = contentType,
                TamanoBytes = archivo.Length,
                FechaSubida = DateTime.UtcNow
            };

            ProductoArchivo? creado = await _productoArchivoRepository.CreateArchivoAsync(nuevoArchivo, cancellationToken);
            if (creado is null)
            {
                await TryDeleteStoredArchivoAsync(nuevoArchivo, cancellationToken);
                return ProductoArchivoOperationResult.UnexpectedError();
            }

            GetProductoArchivoDTO dto = MapToGetProductoArchivoDTO(creado);
            return ProductoArchivoOperationResult.SuccessArchivo(dto);
        }
        catch (MySqlException)
        {
            await TryDeleteStoredArchivoAsync(nuevoArchivo, cancellationToken);
            return ProductoArchivoOperationResult.UnexpectedError();
        }
        catch (IOException)
        {
            await TryDeleteStoredArchivoAsync(nuevoArchivo, cancellationToken);
            return ProductoArchivoOperationResult.UnexpectedError();
        }
        catch (UnauthorizedAccessException)
        {
            await TryDeleteStoredArchivoAsync(nuevoArchivo, cancellationToken);
            return ProductoArchivoOperationResult.UnexpectedError();
        }
    }

    public async Task<ProductoArchivoOperationResult> DeleteArchivoAsync(int productoId, int archivoId, CancellationToken cancellationToken = default)
    {
        if (productoId <= 0 || archivoId <= 0)
        {
            return ProductoArchivoOperationResult.ValidationError();
        }

        Producto? producto;
        try
        {
            producto = await _productoRepository.GetProductoByIdAsync(productoId, cancellationToken);
        }
        catch (MySqlException)
        {
            return ProductoArchivoOperationResult.UnexpectedError();
        }

        if (producto is null)
        {
            return ProductoArchivoOperationResult.NotFoundError();
        }

        ProductoArchivo? archivo;
        try
        {
            archivo = await _productoArchivoRepository.GetArchivoByIdAsync(archivoId, cancellationToken);
        }
        catch (MySqlException)
        {
            return ProductoArchivoOperationResult.UnexpectedError();
        }

        if (archivo is null || archivo.ProductoId != productoId)
        {
            return ProductoArchivoOperationResult.NotFoundError();
        }

        bool deleted;
        try
        {
            deleted = await _productoArchivoRepository.DeleteArchivoAsync(archivoId, cancellationToken);
        }
        catch (MySqlException)
        {
            return ProductoArchivoOperationResult.UnexpectedError();
        }

        if (!deleted)
        {
            return ProductoArchivoOperationResult.NotFoundError();
        }

        try
        {
            await _storageService.DeleteAsync(archivo, cancellationToken);
        }
        catch (IOException)
        {
            return ProductoArchivoOperationResult.UnexpectedError();
        }
        catch (UnauthorizedAccessException)
        {
            return ProductoArchivoOperationResult.UnexpectedError();
        }

        return ProductoArchivoOperationResult.Success();
    }

    private async Task<bool> CanAccessProductoAsync(int productoId, bool includeHidden, CancellationToken cancellationToken)
    {
        Producto? producto = await _productoRepository.GetProductoByIdAsync(productoId, cancellationToken);
        if (producto is null)
        {
            return false;
        }

        return includeHidden || producto.EsVisible;
    }

    private GetProductoArchivoDTO MapToGetProductoArchivoDTO(ProductoArchivo archivo) =>
        new()
        {
            Id = archivo.Id,
            ProductoId = archivo.ProductoId,
            SubidoPorUsuarioId = archivo.SubidoPorUsuarioId,
            NombreOriginal = archivo.NombreOriginal,
            ContentType = archivo.ContentType,
            TamanoBytes = archivo.TamanoBytes,
            FechaSubida = archivo.FechaSubida,
            UrlDescarga = $"/api/productos/{archivo.ProductoId}/archivos/{archivo.Id}"
        };

    private async Task TryDeleteStoredArchivoAsync(ProductoArchivo? archivo, CancellationToken cancellationToken)
    {
        if (archivo is null)
        {
            return;
        }

        try
        {
            await _storageService.DeleteAsync(archivo, cancellationToken);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
