using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public interface IUsuarioService
{
    Task<List<GetUsuarioDTO>> GetUsuariosAsync(GetUsuariosFiltroDTO filtro);
    Task<GetUsuarioDTO?> GetUsuarioByIdAsync(int id);
    Task<UsuarioOperationResult> PostUsuarioAsync(PostUsuarioDTO usuario);
    Task<UsuarioOperationResult> PutUsuarioAsync(PutUsuarioDTO usuario);
    Task<UsuarioOperationResult> DeleteUsuarioAsync(int id);
}
