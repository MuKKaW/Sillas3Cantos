using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public interface IUsuarioService
{
    Task<List<Usuario>> GetUsuariosAsync(int idUsuario, string nombre, bool orderAsc);
    Task<bool> PostUsuarioAsync(Usuario usuario);
    Task<bool> PutUsuarioAsync(Usuario usuario);
    Task<bool> DeleteUsuarioAsync(int id);
}
