using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public interface ISolucionRepository
{
    Task<List<Solucion>> GetSolucionesAsync(int idSolucion, string titulo, bool orderAscent, CancellationToken cancellationToken = default);
    Task<Solucion?> GetSolucionByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Solucion?> CreateSolucionAsync(Solucion solucion, CancellationToken cancellationToken = default);
    Task<bool> UpdateSolucionAsync(Solucion solucion, CancellationToken cancellationToken = default);
    Task<bool> DeleteSolucionAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByTituloAsync(string titulo, int? excludeId = null, CancellationToken cancellationToken = default);
}
