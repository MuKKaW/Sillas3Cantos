using Microsoft.AspNetCore.Http;

namespace SillasTresCantos.Api.Services;

public interface IProductoArchivoService
{
    Task<ProductoArchivoOperationResult> GetArchivosAsync(int productoId, bool includeHidden, CancellationToken cancellationToken = default);
    Task<ProductoArchivoOperationResult> GetArchivoDescargaAsync(int productoId, int archivoId, bool includeHidden, CancellationToken cancellationToken = default);
    Task<ProductoArchivoOperationResult> UploadArchivoAsync(int productoId, IFormFile? archivo, int subidoPorUsuarioId, CancellationToken cancellationToken = default);
    Task<ProductoArchivoOperationResult> DeleteArchivoAsync(int productoId, int archivoId, CancellationToken cancellationToken = default);
}
