using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public interface IProductoRepository
{
    Task<List<Producto>> GetProductosAsync(int idProducto, string nombre, int categoriaId, int marcaId, int creadoPorUsuarioId, bool orderAscent, CancellationToken cancellationToken = default);
    Task<Producto?> GetProductoByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Producto?> CreateProductoAsync(Producto producto, CancellationToken cancellationToken = default);
    Task<bool> UpdateProductoAsync(Producto producto, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductoAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CategoriaExistsAsync(int categoriaId, CancellationToken cancellationToken = default);
    Task<bool> MarcaExistsAsync(int marcaId, CancellationToken cancellationToken = default);
}
