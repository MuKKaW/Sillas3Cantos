using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public interface IProductoArchivoRepository
{
    Task<List<ProductoArchivo>> GetArchivosByProductoIdAsync(int productoId, CancellationToken cancellationToken = default);
    Task<ProductoArchivo?> GetArchivoByIdAsync(int archivoId, CancellationToken cancellationToken = default);
    Task<ProductoArchivo?> CreateArchivoAsync(ProductoArchivo archivo, CancellationToken cancellationToken = default);
    Task<bool> DeleteArchivoAsync(int archivoId, CancellationToken cancellationToken = default);
}
