using SillasTresCantos.Api.DTOs;

namespace SillasTresCantos.Api.Services;

public interface IUsuarioService
{
    Task<List<GetUsuarioDTO>> GetUsuariosAsync(GetUsuariosFiltroDTO filtro);
    Task<bool> PostUsuarioAsync(PostUsuarioDTO usuario);
    Task<bool> PutUsuarioAsync(PutUsuarioDTO usuario);
    Task<bool> DeleteUsuarioAsync(int id);
}
