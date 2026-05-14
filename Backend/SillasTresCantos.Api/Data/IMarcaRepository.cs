using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public interface IMarcaRepository
{
    Task<List<Marca>> GetMarcasAsync(int idMarca, string nombre, bool orderAscent, CancellationToken cancellationToken = default);
    Task<Marca?> GetMarcaByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Marca?> CreateMarcaAsync(Marca marca, CancellationToken cancellationToken = default);
    Task<bool> UpdateMarcaAsync(Marca marca, CancellationToken cancellationToken = default);
    Task<bool> DeleteMarcaAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNombreAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default);
}
