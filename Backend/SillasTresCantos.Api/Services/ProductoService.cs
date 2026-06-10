using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Configuration;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;
using SillasTresCantos.Api.Services.Storage;

namespace SillasTresCantos.Api.Services;

public class ProductoService : IProductoService
{
    private const int MaxNombreLength = 150;
    private const int MaxDescripcionLength = 500;
    private const decimal MaxPrecio = 99999999.99m;
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };
    private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly IProductoRepository _productoRepository;
    private readonly IProductoImagenStorageService _productoImagenStorageService;
    private readonly FileStorageOptions _fileStorageOptions;

    public ProductoService(
        IProductoRepository productoRepository,
        IProductoImagenStorageService productoImagenStorageService,
        IOptions<FileStorageOptions> fileStorageOptions)
    {
        _productoRepository = productoRepository;
        _productoImagenStorageService = productoImagenStorageService;
        _fileStorageOptions = fileStorageOptions.Value;
    }

    public async Task<List<GetProductoDTO>> GetProductosAsync(GetProductosFiltroDTO filtro)
    {
        List<Producto> productos = await _productoRepository.GetProductosAsync(
            filtro.IdProducto,
            filtro.Nombre,
            filtro.CategoriaId,
            filtro.MarcaId,
            filtro.CreadoPorUsuarioId,
            filtro.OrderAscent);

        if (!filtro.IncludeHidden)
        {
            productos = productos
                .Where(producto => producto.EsVisible)
                .ToList();
        }

        return productos
            .Select(MapToGetProductoDTO)
            .ToList();
    }

    public async Task<GetProductoDTO?> GetProductoByIdAsync(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        Producto? existente = await _productoRepository.GetProductoByIdAsync(id);
        if (existente is null)
        {
            return null;
        }

        return MapToGetProductoDTO(existente);
    }

    public async Task<ProductoOperationResult> PostProductoAsync(PostProductoDTO producto, int creadoPorUsuarioId)
    {
        string nombre = producto.Nombre?.Trim() ?? string.Empty;
        string? descripcion = NormalizeOptional(producto.Descripcion);
        bool esVisible = producto.EsVisible ?? true;

        if (!IsValidNombre(nombre)
            || !IsValidDescripcion(descripcion)
            || !IsValidPrecio(producto.Precio)
            || !IsValidStock(producto.Stock)
            || !IsValidForeignId(creadoPorUsuarioId)
            || !IsValidForeignId(producto.CategoriaId)
            || !IsValidForeignId(producto.MarcaId))
        {
            return ProductoOperationResult.ValidationError();
        }

        bool categoriaExiste = await _productoRepository.CategoriaExistsAsync(producto.CategoriaId);
        bool marcaExiste = await _productoRepository.MarcaExistsAsync(producto.MarcaId);

        if (!categoriaExiste || !marcaExiste)
        {
            return ProductoOperationResult.RelatedNotFoundError();
        }

        Producto nuevoProducto = new()
        {
            Id = 0,
            Nombre = nombre,
            Descripcion = descripcion,
            Precio = producto.Precio,
            Stock = producto.Stock,
            CategoriaId = producto.CategoriaId,
            MarcaId = producto.MarcaId,
            CreadoPorUsuarioId = creadoPorUsuarioId,
            EsVisible = esVisible,
            ImagenUrl = null,
            ImagenPublicId = null,
            ImagenResourceType = null,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = null
        };

        Producto? creado;
        try
        {
            creado = await _productoRepository.CreateProductoAsync(nuevoProducto);
        }
        catch (MySqlException ex) when (IsDuplicateKey(ex))
        {
            return ProductoOperationResult.ConflictError();
        }
        catch (MySqlException ex) when (IsForeignKeyViolation(ex))
        {
            return ProductoOperationResult.RelatedNotFoundError();
        }
        catch (MySqlException)
        {
            return ProductoOperationResult.UnexpectedError();
        }

        if (creado is null)
        {
            return ProductoOperationResult.UnexpectedError();
        }

        return ProductoOperationResult.Success(MapToGetProductoDTO(creado));
    }

    public async Task<ProductoOperationResult> PostProductoConImagenAsync(
        PostProductoConImagenDTO producto,
        int creadoPorUsuarioId,
        CancellationToken cancellationToken = default)
    {
        string nombre = producto.Nombre?.Trim() ?? string.Empty;
        string? descripcion = NormalizeOptional(producto.Descripcion);
        bool esVisible = producto.EsVisible ?? true;

        if (!IsValidNombre(nombre)
            || !IsValidDescripcion(descripcion)
            || !IsValidPrecio(producto.Precio)
            || !IsValidStock(producto.Stock)
            || !IsValidForeignId(creadoPorUsuarioId)
            || !IsValidForeignId(producto.CategoriaId)
            || !IsValidForeignId(producto.MarcaId)
            || producto.Imagen is null
            || producto.Imagen.Length == 0)
        {
            return ProductoOperationResult.ValidationError();
        }

        if (producto.Imagen.Length > GetMaxImageSizeBytes())
        {
            return ProductoOperationResult.FileTooLargeError();
        }

        if (!IsSupportedImage(producto.Imagen))
        {
            return ProductoOperationResult.UnsupportedTypeError();
        }

        bool categoriaExiste = await _productoRepository.CategoriaExistsAsync(producto.CategoriaId, cancellationToken);
        bool marcaExiste = await _productoRepository.MarcaExistsAsync(producto.MarcaId, cancellationToken);

        if (!categoriaExiste || !marcaExiste)
        {
            return ProductoOperationResult.RelatedNotFoundError();
        }

        StoredProductoImagen? storedImagen = null;
        try
        {
            storedImagen = await _productoImagenStorageService.UploadAsync(producto.Imagen, cancellationToken);

            Producto nuevoProducto = new()
            {
                Id = 0,
                Nombre = nombre,
                Descripcion = descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                CategoriaId = producto.CategoriaId,
                MarcaId = producto.MarcaId,
                CreadoPorUsuarioId = creadoPorUsuarioId,
                EsVisible = esVisible,
                ImagenUrl = storedImagen.Url,
                ImagenPublicId = storedImagen.PublicId,
                ImagenResourceType = storedImagen.ResourceType,
                FechaCreacion = DateTime.UtcNow,
                FechaActualizacion = null
            };

            Producto? creado = await _productoRepository.CreateProductoAsync(nuevoProducto, cancellationToken);
            if (creado is null)
            {
                await TryDeleteStoredImagenAsync(storedImagen, CancellationToken.None);
                return ProductoOperationResult.UnexpectedError();
            }

            return ProductoOperationResult.Success(MapToGetProductoDTO(creado));
        }
        catch (MySqlException ex) when (IsDuplicateKey(ex))
        {
            await TryDeleteStoredImagenAsync(storedImagen, CancellationToken.None);
            return ProductoOperationResult.ConflictError();
        }
        catch (MySqlException ex) when (IsForeignKeyViolation(ex))
        {
            await TryDeleteStoredImagenAsync(storedImagen, CancellationToken.None);
            return ProductoOperationResult.RelatedNotFoundError();
        }
        catch (MySqlException)
        {
            await TryDeleteStoredImagenAsync(storedImagen, CancellationToken.None);
            return ProductoOperationResult.UnexpectedError();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await TryDeleteStoredImagenAsync(storedImagen, CancellationToken.None);
            throw;
        }
        catch (Exception)
        {
            await TryDeleteStoredImagenAsync(storedImagen, CancellationToken.None);
            return ProductoOperationResult.UnexpectedError();
        }
    }

    public async Task<ProductoOperationResult> PutProductoAsync(PutProductoDTO producto)
    {
        if (producto.Id <= 0)
        {
            return ProductoOperationResult.ValidationError();
        }

        Producto? existente = await _productoRepository.GetProductoByIdAsync(producto.Id);
        if (existente is null)
        {
            return ProductoOperationResult.NotFoundError();
        }

        if (producto.Nombre is not null && string.IsNullOrWhiteSpace(producto.Nombre))
        {
            return ProductoOperationResult.ValidationError();
        }

        if (producto.CategoriaId.HasValue && !IsValidForeignId(producto.CategoriaId.Value))
        {
            return ProductoOperationResult.ValidationError();
        }

        if (producto.MarcaId.HasValue && !IsValidForeignId(producto.MarcaId.Value))
        {
            return ProductoOperationResult.ValidationError();
        }

        if (producto.Precio.HasValue && !IsValidPrecio(producto.Precio.Value))
        {
            return ProductoOperationResult.ValidationError();
        }

        if (producto.Stock.HasValue && !IsValidStock(producto.Stock.Value))
        {
            return ProductoOperationResult.ValidationError();
        }

        if (producto.Descripcion is not null && producto.Descripcion.Length > MaxDescripcionLength)
        {
            return ProductoOperationResult.ValidationError();
        }

        string nombreFinal = string.IsNullOrWhiteSpace(producto.Nombre)
            ? existente.Nombre
            : producto.Nombre.Trim();
        string? descripcionFinal = producto.Descripcion is null
            ? existente.Descripcion
            : NormalizeOptional(producto.Descripcion);
        decimal precioFinal = producto.Precio ?? existente.Precio;
        int stockFinal = producto.Stock ?? existente.Stock;
        int categoriaIdFinal = producto.CategoriaId ?? existente.CategoriaId;
        int marcaIdFinal = producto.MarcaId ?? existente.MarcaId;
        bool esVisibleFinal = producto.EsVisible ?? existente.EsVisible;

        if (!IsValidNombre(nombreFinal)
            || !IsValidDescripcion(descripcionFinal)
            || !IsValidPrecio(precioFinal)
            || !IsValidStock(stockFinal)
            || !IsValidForeignId(categoriaIdFinal)
            || !IsValidForeignId(marcaIdFinal))
        {
            return ProductoOperationResult.ValidationError();
        }

        bool categoriaExiste = await _productoRepository.CategoriaExistsAsync(categoriaIdFinal);
        bool marcaExiste = await _productoRepository.MarcaExistsAsync(marcaIdFinal);

        if (!categoriaExiste || !marcaExiste)
        {
            return ProductoOperationResult.RelatedNotFoundError();
        }

        Producto actualizado = new()
        {
            Id = existente.Id,
            Nombre = nombreFinal,
            Descripcion = descripcionFinal,
            Precio = precioFinal,
            Stock = stockFinal,
            CategoriaId = categoriaIdFinal,
            MarcaId = marcaIdFinal,
            CreadoPorUsuarioId = existente.CreadoPorUsuarioId,
            EsVisible = esVisibleFinal,
            ImagenUrl = existente.ImagenUrl,
            ImagenPublicId = existente.ImagenPublicId,
            ImagenResourceType = existente.ImagenResourceType,
            FechaCreacion = existente.FechaCreacion,
            FechaActualizacion = DateTime.UtcNow
        };

        bool updated;
        try
        {
            updated = await _productoRepository.UpdateProductoAsync(actualizado);
        }
        catch (MySqlException ex) when (IsDuplicateKey(ex))
        {
            return ProductoOperationResult.ConflictError();
        }
        catch (MySqlException ex) when (IsForeignKeyViolation(ex))
        {
            return ProductoOperationResult.RelatedNotFoundError();
        }
        catch (MySqlException)
        {
            return ProductoOperationResult.UnexpectedError();
        }

        if (!updated)
        {
            return ProductoOperationResult.NotFoundError();
        }

        return ProductoOperationResult.Success(MapToGetProductoDTO(actualizado));
    }

    public async Task<ProductoOperationResult> DeleteProductoAsync(int id)
    {
        if (id <= 0)
        {
            return ProductoOperationResult.ValidationError();
        }

        bool deleted;
        try
        {
            deleted = await _productoRepository.DeleteProductoAsync(id);
        }
        catch (MySqlException ex) when (IsForeignKeyDeleteViolation(ex))
        {
            return ProductoOperationResult.ConflictError();
        }
        catch (MySqlException)
        {
            return ProductoOperationResult.UnexpectedError();
        }

        if (!deleted)
        {
            return ProductoOperationResult.NotFoundError();
        }

        return ProductoOperationResult.Success();
    }

    private static GetProductoDTO MapToGetProductoDTO(Producto producto) =>
        new()
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            Precio = producto.Precio,
            Stock = producto.Stock,
            CategoriaId = producto.CategoriaId,
            MarcaId = producto.MarcaId,
            CreadoPorUsuarioId = producto.CreadoPorUsuarioId,
            EsVisible = producto.EsVisible,
            ImagenUrl = producto.ImagenUrl,
            FechaCreacion = producto.FechaCreacion,
            FechaActualizacion = producto.FechaActualizacion
        };

    private static string? NormalizeOptional(string? value)
    {
        if (value is null)
        {
            return null;
        }

        string normalized = value.Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    private static bool IsValidNombre(string nombre) =>
        !string.IsNullOrWhiteSpace(nombre) && nombre.Length <= MaxNombreLength;

    private static bool IsValidDescripcion(string? descripcion) =>
        descripcion is null || descripcion.Length <= MaxDescripcionLength;

    private static bool IsValidPrecio(decimal precio) =>
        precio >= 0m && precio <= MaxPrecio;

    private static bool IsValidStock(int stock) =>
        stock >= 0;

    private static bool IsValidForeignId(int id) =>
        id > 0;

    private long GetMaxImageSizeBytes() =>
        _fileStorageOptions.MaxFileSizeBytes <= 0
            ? 5 * 1024 * 1024
            : _fileStorageOptions.MaxFileSizeBytes;

    private static bool IsSupportedImage(IFormFile imagen)
    {
        string extension = Path.GetExtension(imagen.FileName);
        if (!AllowedImageExtensions.Contains(extension))
        {
            return false;
        }

        return AllowedImageContentTypes.Contains(imagen.ContentType);
    }

    private async Task TryDeleteStoredImagenAsync(StoredProductoImagen? storedImagen, CancellationToken cancellationToken)
    {
        if (storedImagen is null)
        {
            return;
        }

        try
        {
            await _productoImagenStorageService.DeleteAsync(storedImagen.PublicId, cancellationToken);
        }
        catch (Exception)
        {
        }
    }

    private static bool IsDuplicateKey(MySqlException ex) => ex.Number == 1062;

    private static bool IsForeignKeyViolation(MySqlException ex) => ex.Number == 1452;

    private static bool IsForeignKeyDeleteViolation(MySqlException ex) => ex.Number == 1451;
}
