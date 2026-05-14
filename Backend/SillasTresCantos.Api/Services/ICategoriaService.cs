using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public interface ICategoriaService
{
    Task<List<GetCategoriaDTO>> GetCategoriasAsync(GetCategoriasFiltroDTO filtro);
    Task<GetCategoriaDTO?> GetCategoriaByIdAsync(int id);
    Task<CategoriaOperationResult> PostCategoriaAsync(PostCategoriaDTO categoria);
    Task<CategoriaOperationResult> PutCategoriaAsync(PutCategoriaDTO categoria);
    Task<CategoriaOperationResult> DeleteCategoriaAsync(int id);
}
