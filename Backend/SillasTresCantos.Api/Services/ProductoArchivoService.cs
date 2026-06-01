using MySql.Data.MySqlClient;
using Microsoft.Extensions.Options;
using SillasTresCantos.Api.Configuration;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public class ProductoArchivoService : IProductoArchivoService
{
    private const long DefaultMaxFileSizeBytes = 5 * 1024 * 1024;
    private readonly IProductoArchivoRepository _productoArchivoRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly FileStorageOptions _fileStorageOptions;
    private readonly IWebHostEnvironment _environment;
    private readonly HashSet<string> _allowedExtensions;

    public ProductoArchivoService(
        IProductoArchivoRepository productoArchivoRepository,
        IProductoRepository productoRepository,
        IOptions<FileStorageOptions> fileStorageOptions,
        IWebHostEnvironment environment)
    {
        _productoArchivoRepository = productoArchivoRepository;
        _productoRepository = productoRepository;
        _fileStorageOptions = fileStorageOptions.Value;
        _environment = environment;
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

        if (!TryResolveAbsolutePath(archivo.RutaRelativa, out string absolutePath))
        {
            return ProductoArchivoOperationResult.NotFoundError();
        }

        if (!File.Exists(absolutePath))
        {
            return ProductoArchivoOperationResult.NotFoundError();
        }

        GetProductoArchivoDTO dto = MapToGetProductoArchivoDTO(archivo);
        string contentType = string.IsNullOrWhiteSpace(archivo.ContentType)
            ? "application/octet-stream"
            : archivo.ContentType;

        return ProductoArchivoOperationResult.SuccessDescarga(
            dto,
            absolutePath,
            contentType,
            archivo.NombreOriginal);
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

        string? absolutePath = null;
        try
        {
            string rootPath = GetStorageRootAbsolutePath();
            string productoPath = Path.Combine(rootPath, "productos", productoId.ToString());
            Directory.CreateDirectory(productoPath);

            string generatedFileName = $"{Guid.NewGuid():N}{extension}";
            absolutePath = Path.Combine(productoPath, generatedFileName);

            await using (FileStream outputStream = new(absolutePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await archivo.CopyToAsync(outputStream, cancellationToken);
            }

            string relativePath = Path.GetRelativePath(rootPath, absolutePath).Replace("\\", "/");
            string contentType = string.IsNullOrWhiteSpace(archivo.ContentType)
                ? "application/octet-stream"
                : archivo.ContentType;

            ProductoArchivo nuevoArchivo = new()
            {
                Id = 0,
                ProductoId = productoId,
                SubidoPorUsuarioId = subidoPorUsuarioId,
                NombreOriginal = Path.GetFileName(archivo.FileName),
                NombreAlmacenado = generatedFileName,
                RutaRelativa = relativePath,
                ContentType = contentType,
                TamanoBytes = archivo.Length,
                FechaSubida = DateTime.UtcNow
            };

            ProductoArchivo? creado = await _productoArchivoRepository.CreateArchivoAsync(nuevoArchivo, cancellationToken);
            if (creado is null)
            {
                TryDeleteFile(absolutePath);
                return ProductoArchivoOperationResult.UnexpectedError();
            }

            GetProductoArchivoDTO dto = MapToGetProductoArchivoDTO(creado);
            return ProductoArchivoOperationResult.SuccessArchivo(dto);
        }
        catch (MySqlException)
        {
            TryDeleteFile(absolutePath);
            return ProductoArchivoOperationResult.UnexpectedError();
        }
        catch (IOException)
        {
            TryDeleteFile(absolutePath);
            return ProductoArchivoOperationResult.UnexpectedError();
        }
        catch (UnauthorizedAccessException)
        {
            TryDeleteFile(absolutePath);
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

        if (TryResolveAbsolutePath(archivo.RutaRelativa, out string absolutePath))
        {
            TryDeleteFile(absolutePath);
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
