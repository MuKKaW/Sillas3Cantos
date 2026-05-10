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
            .Select(u => new GetUsuarioDTO
            {
                Id = u.Id,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email,
                FechaRegistro = u.FechaRegistro,
                EstaActivo = u.EstaActivo
            })
            .ToList();

        return Task.FromResult(usuarios);
    }

    public Task<bool> PostUsuarioAsync(PostUsuarioDTO usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Email))
        {
            return Task.FromResult(false);
        }

        if (Usuarios.Any(u => string.Equals(u.Email, usuario.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.FromResult(false);
        }

        int newId = _nextId++;

        Usuarios.Add(new Usuario
        {
            Id = newId,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            FechaRegistro = DateTime.UtcNow,
            EstaActivo = true
        });

        return Task.FromResult(true);
    }

    public Task<bool> PutUsuarioAsync(PutUsuarioDTO usuario)
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
