using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public interface ISolucionService
{
    Task<List<GetSolucionDTO>> GetSolucionesAsync(GetSolucionesFiltroDTO filtro);
    Task<GetSolucionDTO?> GetSolucionByIdAsync(int id);
    Task<SolucionOperationResult> PostSolucionAsync(PostSolucionDTO solucion);
    Task<SolucionOperationResult> PutSolucionAsync(PutSolucionDTO solucion);
    Task<SolucionOperationResult> DeleteSolucionAsync(int id);
}
