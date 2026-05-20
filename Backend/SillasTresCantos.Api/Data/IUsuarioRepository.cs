using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public interface IUsuarioRepository
{
    Task<List<Usuario>> GetUsuariosAsync(int idUsuario, string nombre, bool orderAscent, CancellationToken cancellationToken = default);
    Task<Usuario?> GetUsuarioByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Usuario?> CreateUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task<bool> UpdateUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task<bool> DeleteUsuarioAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<Usuario?> GetUsuarioByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(string username, int? excludeId = null, CancellationToken cancellationToken = default);
}
