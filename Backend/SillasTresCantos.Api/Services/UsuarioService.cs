using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<List<GetUsuarioDTO>> GetUsuariosAsync(GetUsuariosFiltroDTO filtro)
    {
        List<Usuario> usuarios = await _usuarioRepository.GetUsuariosAsync(filtro.IdUsuario, filtro.Nombre, filtro.OrderAscent);
        return usuarios
            .Select(MapToGetUsuarioDTO)
            .ToList();
    }

    public async Task<GetUsuarioDTO?> GetUsuarioByIdAsync(int id)
    {
        if (id <= 0)
        {
            return null;
        }

        Usuario? existente = await _usuarioRepository.GetUsuarioByIdAsync(id);
        if (existente is null)
        {
            return null;
        }

        return MapToGetUsuarioDTO(existente);
    }

    public async Task<UsuarioOperationResult> PostUsuarioAsync(PostUsuarioDTO usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Email))
        {
            return UsuarioOperationResult.ValidationError();
        }

        bool existeEmail = await _usuarioRepository.ExistsByEmailAsync(usuario.Email);
        if (existeEmail)
        {
            return UsuarioOperationResult.ConflictError();
        }

        Usuario nuevoUsuario = new()
        {
            Id = 0,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            FechaRegistro = DateTime.UtcNow,
            EstaActivo = true
        };

        Usuario? creado = await _usuarioRepository.CreateUsuarioAsync(nuevoUsuario);
        if (creado is null)
        {
            return UsuarioOperationResult.Success();
        }

        return UsuarioOperationResult.Success(MapToGetUsuarioDTO(creado));
    }

    public async Task<UsuarioOperationResult> PutUsuarioAsync(PutUsuarioDTO usuario)
    {
        if (usuario.Id <= 0)
        {
            return UsuarioOperationResult.ValidationError();
        }

        Usuario? existente = await _usuarioRepository.GetUsuarioByIdAsync(usuario.Id);
        if (existente is null)
        {
            return UsuarioOperationResult.NotFoundError();
        }

        if (usuario.Email is not null && string.IsNullOrWhiteSpace(usuario.Email))
        {
            return UsuarioOperationResult.ValidationError();
        }

        string emailFinal = string.IsNullOrWhiteSpace(usuario.Email) ? existente.Email : usuario.Email;

        bool emailDuplicado = await _usuarioRepository.ExistsByEmailAsync(emailFinal, usuario.Id);
        if (emailDuplicado)
        {
            return UsuarioOperationResult.ConflictError();
        }

        Usuario actualizado = new()
        {
            Id = existente.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = emailFinal,
            FechaRegistro = existente.FechaRegistro,
            EstaActivo = usuario.EstaActivo ?? existente.EstaActivo
        };

        bool updated = await _usuarioRepository.UpdateUsuarioAsync(actualizado);
        if (!updated)
        {
            return UsuarioOperationResult.NotFoundError();
        }

        return UsuarioOperationResult.Success(MapToGetUsuarioDTO(actualizado));
    }

    public async Task<UsuarioOperationResult> DeleteUsuarioAsync(int id)
    {
        if (id <= 0)
        {
            return UsuarioOperationResult.ValidationError();
        }

        bool deleted = await _usuarioRepository.DeleteUsuarioAsync(id);
        if (!deleted)
        {
            return UsuarioOperationResult.NotFoundError();
        }

        return UsuarioOperationResult.Success();
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
