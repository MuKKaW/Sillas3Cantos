using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public interface IMarcaService
{
    Task<List<GetMarcaDTO>> GetMarcasAsync(GetMarcasFiltroDTO filtro);
    Task<GetMarcaDTO?> GetMarcaByIdAsync(int id);
    Task<MarcaOperationResult> PostMarcaAsync(PostMarcaDTO marca);
    Task<MarcaOperationResult> PutMarcaAsync(PutMarcaDTO marca);
    Task<MarcaOperationResult> DeleteMarcaAsync(int id);
}
