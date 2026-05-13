using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.Models;
using MySql.Data.MySqlClient;
using System.Net.Mail;

namespace SillasTresCantos.Api.Services;

public class UsuarioService : IUsuarioService
{
    private const int MaxNombreLength = 100;
    private const int MaxApellidoLength = 100;
    private const int MaxEmailLength = 255;
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
        string? nombre = NormalizeOptional(usuario.Nombre);
        string? apellido = NormalizeOptional(usuario.Apellido);
        string email = usuario.Email?.Trim() ?? string.Empty;

        if (!IsValidNombre(nombre) || !IsValidApellido(apellido) || !IsValidEmail(email))
        {
            return UsuarioOperationResult.ValidationError();
        }

        bool existeEmail = await _usuarioRepository.ExistsByEmailAsync(email);
        if (existeEmail)
        {
            return UsuarioOperationResult.ConflictError();
        }

        Usuario nuevoUsuario = new()
        {
            Id = 0,
            Nombre = nombre,
            Apellido = apellido,
            Email = email,
            FechaRegistro = DateTime.UtcNow,
            EstaActivo = true
        };

        Usuario? creado;
        try
        {
            creado = await _usuarioRepository.CreateUsuarioAsync(nuevoUsuario);
        }
        catch (MySqlException ex) when (IsDuplicateKey(ex))
        {
            return UsuarioOperationResult.ConflictError();
        }
        catch (MySqlException)
        {
            return UsuarioOperationResult.UnexpectedError();
        }

        if (creado is null)
        {
            return UsuarioOperationResult.UnexpectedError();
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

        string? nombre = usuario.Nombre is null ? null : NormalizeOptional(usuario.Nombre);
        string? apellido = usuario.Apellido is null ? null : NormalizeOptional(usuario.Apellido);
        string emailFinal = string.IsNullOrWhiteSpace(usuario.Email) ? existente.Email : usuario.Email.Trim();

        if (!IsValidNombre(nombre) || !IsValidApellido(apellido) || !IsValidEmail(emailFinal))
        {
            return UsuarioOperationResult.ValidationError();
        }

        bool emailDuplicado = await _usuarioRepository.ExistsByEmailAsync(emailFinal, usuario.Id);
        if (emailDuplicado)
        {
            return UsuarioOperationResult.ConflictError();
        }

        Usuario actualizado = new()
        {
            Id = existente.Id,
            Nombre = nombre,
            Apellido = apellido,
            Email = emailFinal,
            FechaRegistro = existente.FechaRegistro,
            EstaActivo = usuario.EstaActivo ?? existente.EstaActivo
        };

        bool updated;
        try
        {
            updated = await _usuarioRepository.UpdateUsuarioAsync(actualizado);
        }
        catch (MySqlException ex) when (IsDuplicateKey(ex))
        {
            return UsuarioOperationResult.ConflictError();
        }
        catch (MySqlException)
        {
            return UsuarioOperationResult.UnexpectedError();
        }

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

    private static string? NormalizeOptional(string? value)
    {
        if (value is null)
        {
            return null;
        }

        string normalized = value.Trim();
        return normalized.Length == 0 ? null : normalized;
    }

    private static bool IsValidNombre(string? nombre) =>
        nombre is null || nombre.Length <= MaxNombreLength;

    private static bool IsValidApellido(string? apellido) =>
        apellido is null || apellido.Length <= MaxApellidoLength;

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > MaxEmailLength)
        {
            return false;
        }

        try
        {
            MailAddress address = new(email);
            return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsDuplicateKey(MySqlException ex) => ex.Number == 1062;
}
