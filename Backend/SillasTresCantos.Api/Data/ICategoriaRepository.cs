using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public interface ICategoriaRepository
{
    Task<List<Categoria>> GetCategoriasAsync(int idCategoria, string nombre, bool orderAscent, CancellationToken cancellationToken = default);
    Task<Categoria?> GetCategoriaByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Categoria?> CreateCategoriaAsync(Categoria categoria, CancellationToken cancellationToken = default);
    Task<bool> UpdateCategoriaAsync(Categoria categoria, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoriaAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNombreAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default);
}
