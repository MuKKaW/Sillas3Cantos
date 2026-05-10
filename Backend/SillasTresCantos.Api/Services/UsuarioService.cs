using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public class UsuarioService : IUsuarioService
{
    private static readonly List<Usuario> Usuarios = [];
    private static int _nextId = 1;

    public Task<List<Usuario>> GetUsuariosAsync(int idUsuario, string nombre, bool orderAsc)
    {
        IEnumerable<Usuario> query = Usuarios;

        if (idUsuario > 0)
        {
            query = query.Where(u => u.Id == idUsuario);
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            query = query.Where(u => (u.Nombre ?? string.Empty).Contains(nombre, StringComparison.OrdinalIgnoreCase));
        }

        query = orderAsc
            ? query.OrderBy(u => u.Nombre ?? string.Empty)
            : query.OrderByDescending(u => u.Nombre ?? string.Empty);

        return Task.FromResult(query.ToList());
    }

    public Task<bool> PostUsuarioAsync(Usuario usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Email))
        {
            return Task.FromResult(false);
        }

        if (Usuarios.Any(u => string.Equals(u.Email, usuario.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(false);
        }

        int newId = usuario.Id > 0 ? usuario.Id : _nextId++;

        if (Usuarios.Any(u => u.Id == newId))
        {
            return Task.FromResult(false);
        }

        if (newId >= _nextId)
        {
            _nextId = newId + 1;
        }

        Usuario nuevoUsuario = new()
        {
            Id = newId,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            FechaRegistro = usuario.FechaRegistro ?? DateTime.UtcNow,
            EstaActivo = usuario.EstaActivo ?? true
        };

        Usuarios.Add(nuevoUsuario);
        return Task.FromResult(true);
    }

    public Task<bool> PutUsuarioAsync(Usuario usuario)
    {
        Usuario? existente = Usuarios.FirstOrDefault(u => u.Id == usuario.Id);
        if (existente is null)
        {
            return Task.FromResult(false);
        }

        if (!string.IsNullOrWhiteSpace(usuario.Email))
        {
            bool emailDuplicado = Usuarios.Any(u => u.Id != usuario.Id && string.Equals(u.Email, usuario.Email, StringComparison.OrdinalIgnoreCase));
            if (emailDuplicado)
            {
                return Task.FromResult(false);
            }
        }

        existente.Nombre = usuario.Nombre;
        existente.Apellido = usuario.Apellido;
        existente.Email = string.IsNullOrWhiteSpace(usuario.Email) ? existente.Email : usuario.Email;
        existente.FechaRegistro = usuario.FechaRegistro ?? existente.FechaRegistro;
        existente.EstaActivo = usuario.EstaActivo ?? existente.EstaActivo;

        return Task.FromResult(true);
    }

    public Task<bool> DeleteUsuarioAsync(int id)
    {
        Usuario? existente = Usuarios.FirstOrDefault(u => u.Id == id);
        if (existente is null)
        {
            return Task.FromResult(false);
        }

        Usuarios.Remove(existente);
        return Task.FromResult(true);
    }
}
