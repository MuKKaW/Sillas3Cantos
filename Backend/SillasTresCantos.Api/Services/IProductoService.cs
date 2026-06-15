using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public interface IProductoService
{
    Task<List<GetProductoDTO>> GetProductosAsync(GetProductosFiltroDTO filtro);
    Task<GetProductoDTO?> GetProductoByIdAsync(int id);
    Task<ProductoOperationResult> PostProductoAsync(PostProductoDTO producto, int creadoPorUsuarioId, CancellationToken cancellationToken = default);
    Task<ProductoOperationResult> PutProductoAsync(PutProductoDTO producto, CancellationToken cancellationToken = default);
    Task<ProductoOperationResult> DeleteProductoAsync(int id);
}
