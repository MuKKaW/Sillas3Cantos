using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Data;
using SillasTresCantos.Api.DTOs;
using SillasTresCantos.Api.Models;
using System.Net.Mail;

namespace SillasTresCantos.Api.Services;

public class UsuarioService : IUsuarioService
{
    private const int MaxUsernameLength = 100;
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 128;
    private const int MaxRoleLength = 30;
    private const int MaxNombreLength = 100;
    private const int MaxApellidoLength = 100;
    private const int MaxEmailLength = 255;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UsuarioService(IUsuarioRepository usuarioRepository, IPasswordHasher passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
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
        string? username = NormalizeOptional(usuario.Username);
        string password = usuario.Password ?? string.Empty;
        string role = NormalizeRole(usuario.Role, "User");
        string? nombre = NormalizeOptional(usuario.Nombre);
        string? apellido = NormalizeOptional(usuario.Apellido);
        string email = usuario.Email?.Trim() ?? string.Empty;

        if (!IsValidUsername(username)
            || !IsValidPassword(password)
            || !IsValidRole(role)
            || !IsValidNombre(nombre)
            || !IsValidApellido(apellido)
            || !IsValidEmail(email))
        {
            return UsuarioOperationResult.ValidationError();
        }

        bool existeEmail = await _usuarioRepository.ExistsByEmailAsync(email);
        if (existeEmail)
        {
            return UsuarioOperationResult.ConflictEmailError();
        }

        bool existeUsername = await _usuarioRepository.ExistsByUsernameAsync(username!);
        if (existeUsername)
        {
            return UsuarioOperationResult.ConflictUsernameError();
        }

        string passwordHash = _passwordHasher.Hash(password);

        Usuario nuevoUsuario = new()
        {
            Id = 0,
            Username = username,
            PasswordHash = passwordHash,
            Role = role,
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
            return MapDuplicateConflict(ex);
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

        if (usuario.Username is not null && string.IsNullOrWhiteSpace(usuario.Username))
        {
            return UsuarioOperationResult.ValidationError();
        }

        if (usuario.Role is not null && string.IsNullOrWhiteSpace(usuario.Role))
        {
            return UsuarioOperationResult.ValidationError();
        }

        if (usuario.Password is not null && string.IsNullOrWhiteSpace(usuario.Password))
        {
            return UsuarioOperationResult.ValidationError();
        }

        string? nombreFinal = usuario.Nombre is null ? existente.Nombre : NormalizeOptional(usuario.Nombre);
        string? apellidoFinal = usuario.Apellido is null ? existente.Apellido : NormalizeOptional(usuario.Apellido);
        string emailFinal = string.IsNullOrWhiteSpace(usuario.Email) ? existente.Email : usuario.Email.Trim();
        string? usernameFinal = usuario.Username is null ? NormalizeOptional(existente.Username) : NormalizeOptional(usuario.Username);
        string roleBase = usuario.Role is null ? existente.Role ?? "User" : usuario.Role;
        string roleFinal = NormalizeRole(roleBase, "User");

        string? passwordHashFinal = existente.PasswordHash;
        if (usuario.Password is not null)
        {
            if (!IsValidPassword(usuario.Password))
            {
                return UsuarioOperationResult.ValidationError();
            }

            passwordHashFinal = _passwordHasher.Hash(usuario.Password);
        }

        if (!IsValidUsername(usernameFinal)
            || !IsValidRole(roleFinal)
            || !IsValidNombre(nombreFinal)
            || !IsValidApellido(apellidoFinal)
            || !IsValidEmail(emailFinal))
        {
            return UsuarioOperationResult.ValidationError();
        }

        bool emailDuplicado = await _usuarioRepository.ExistsByEmailAsync(emailFinal, usuario.Id);
        if (emailDuplicado)
        {
            return UsuarioOperationResult.ConflictEmailError();
        }

        bool usernameDuplicado = await _usuarioRepository.ExistsByUsernameAsync(usernameFinal!, usuario.Id);
        if (usernameDuplicado)
        {
            return UsuarioOperationResult.ConflictUsernameError();
        }

        Usuario actualizado = new()
        {
            Id = existente.Id,
            Username = usernameFinal,
            PasswordHash = passwordHashFinal,
            Role = roleFinal,
            Nombre = nombreFinal,
            Apellido = apellidoFinal,
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
            return MapDuplicateConflict(ex);
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
            Username = usuario.Username,
            Role = usuario.Role,
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

    private static string NormalizeRole(string? value, string defaultRole)
    {
        string? normalized = NormalizeOptional(value);
        if (normalized is null)
        {
            return defaultRole;
        }

        if (normalized.Equals("admin", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("superadmin", StringComparison.OrdinalIgnoreCase))
        {
            return "SuperAdmin";
        }

        if (normalized.Equals("user", StringComparison.OrdinalIgnoreCase))
        {
            return "User";
        }

        return normalized;
    }

    private static bool IsValidUsername(string? username) =>
        !string.IsNullOrWhiteSpace(username) && username.Length <= MaxUsernameLength;

    private static bool IsValidPassword(string password) =>
        !string.IsNullOrWhiteSpace(password)
        && password.Length >= MinPasswordLength
        && password.Length <= MaxPasswordLength;

    private static bool IsValidRole(string role) =>
        !string.IsNullOrWhiteSpace(role)
        && role.Length <= MaxRoleLength
        && (role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)
            || role.Equals("User", StringComparison.OrdinalIgnoreCase));

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

    private static UsuarioOperationResult MapDuplicateConflict(MySqlException ex)
    {
        if (ex.Message.Contains("uq_usuarios_email", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("email", StringComparison.OrdinalIgnoreCase))
        {
            return UsuarioOperationResult.ConflictEmailError();
        }

        if (ex.Message.Contains("uq_usuarios_username", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("username", StringComparison.OrdinalIgnoreCase))
        {
            return UsuarioOperationResult.ConflictUsernameError();
        }

        return UsuarioOperationResult.ConflictError();
    }

    private static bool IsDuplicateKey(MySqlException ex) => ex.Number == 1062;
}
