using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public class UsuarioService : IUsuarioService
{
    private static readonly List<Usuario> Usuarios = [];
    private static int _nextId = 1;

    public Task<List<GetUsuarioDTO>> GetUsuariosAsync(GetUsuariosFiltroDTO filtro)
    {
        IEnumerable<Usuario> query = Usuarios;

        if (filtro.IdUsuario > 0)
        {
            query = query.Where(u => u.Id == filtro.IdUsuario);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Nombre))
        {
            query = query.Where(u => (u.Nombre ?? string.Empty).Contains(filtro.Nombre, StringComparison.OrdinalIgnoreCase));
        }

        query = filtro.OrderAscent
            ? query.OrderBy(u => u.Nombre ?? string.Empty)
            : query.OrderByDescending(u => u.Nombre ?? string.Empty);

        List<GetUsuarioDTO> usuarios = query
            .Select(MapToGetUsuarioDTO)
            .ToList();

        return Task.FromResult(usuarios);
    }

    public Task<GetUsuarioDTO?> GetUsuarioByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Task.FromResult<GetUsuarioDTO?>(null);
        }

        Usuario? existente = Usuarios.FirstOrDefault(u => u.Id == id);
        if (existente is null)
        {
            return Task.FromResult<GetUsuarioDTO?>(null);
        }

        return Task.FromResult<GetUsuarioDTO?>(MapToGetUsuarioDTO(existente));
    }

    public Task<UsuarioOperationResult> PostUsuarioAsync(PostUsuarioDTO usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Email))
        {
            return Task.FromResult(UsuarioOperationResult.ValidationError());
        }

        if (Usuarios.Any(u => string.Equals(u.Email, usuario.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(UsuarioOperationResult.ConflictError());
        }

        int newId = _nextId++;

        Usuario nuevoUsuario = new()
        {
            Id = newId,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            FechaRegistro = DateTime.UtcNow,
            EstaActivo = true
        };

        Usuarios.Add(nuevoUsuario);

        return Task.FromResult(UsuarioOperationResult.Success(MapToGetUsuarioDTO(nuevoUsuario)));
    }

    public Task<UsuarioOperationResult> PutUsuarioAsync(PutUsuarioDTO usuario)
    {
        if (usuario.Id <= 0)
        {
            return Task.FromResult(UsuarioOperationResult.ValidationError());
        }

        Usuario? existente = Usuarios.FirstOrDefault(u => u.Id == usuario.Id);
        if (existente is null)
        {
            return Task.FromResult(UsuarioOperationResult.NotFoundError());
        }

        if (usuario.Email is not null && string.IsNullOrWhiteSpace(usuario.Email))
        {
            return Task.FromResult(UsuarioOperationResult.ValidationError());
        }

        if (!string.IsNullOrWhiteSpace(usuario.Email))
        {
            bool emailDuplicado = Usuarios.Any(u => u.Id != usuario.Id && string.Equals(u.Email, usuario.Email, StringComparison.OrdinalIgnoreCase));
            if (emailDuplicado)
            {
                return Task.FromResult(UsuarioOperationResult.ConflictError());
            }
        }

        existente.Nombre = usuario.Nombre;
        existente.Apellido = usuario.Apellido;
        existente.Email = string.IsNullOrWhiteSpace(usuario.Email) ? existente.Email : usuario.Email;
        existente.EstaActivo = usuario.EstaActivo ?? existente.EstaActivo;

        return Task.FromResult(UsuarioOperationResult.Success(MapToGetUsuarioDTO(existente)));
    }

    public Task<UsuarioOperationResult> DeleteUsuarioAsync(int id)
    {
        if (id <= 0)
        {
            return Task.FromResult(UsuarioOperationResult.ValidationError());
        }

        Usuario? existente = Usuarios.FirstOrDefault(u => u.Id == id);
        if (existente is null)
        {
            return Task.FromResult(UsuarioOperationResult.NotFoundError());
        }

        Usuarios.Remove(existente);
        return Task.FromResult(UsuarioOperationResult.Success());
    }

    private static GetUsuarioDTO MapToGetUsuarioDTO(Usuario usuario) =>
        new()
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            FechaRegistro = usuario.FechaRegistro,
            EstaActivo = usuario.EstaActivo
        };
}
