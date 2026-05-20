using System.Data.Common;
using System.Text;
using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public class MySqlUsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MySqlUsuarioRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Usuario>> GetUsuariosAsync(int idUsuario, string nombre, bool orderAscent, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        StringBuilder sql = new("SELECT id, username, password_hash, role, nombre, apellido, email, fecha_registro, esta_activo FROM usuarios");
        List<string> whereClauses = [];

        if (idUsuario > 0)
        {
            whereClauses.Add("id = @idUsuario");
            command.Parameters.AddWithValue("@idUsuario", idUsuario);
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            whereClauses.Add("nombre LIKE @nombre");
            command.Parameters.AddWithValue("@nombre", $"%{nombre}%");
        }

        if (whereClauses.Count > 0)
        {
            sql.Append(" WHERE ").Append(string.Join(" AND ", whereClauses));
        }

        sql.Append(orderAscent
            ? " ORDER BY COALESCE(nombre, '') ASC"
            : " ORDER BY COALESCE(nombre, '') DESC");

        command.CommandText = sql.ToString();

        List<Usuario> usuarios = [];
        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            usuarios.Add(MapToUsuario(reader));
        }

        return usuarios;
    }

    public async Task<Usuario?> GetUsuarioByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              SELECT id, username, password_hash, role, nombre, apellido, email, fecha_registro, esta_activo
                              FROM usuarios
                              WHERE id = @id
                              LIMIT 1;
                              """;
        command.Parameters.AddWithValue("@id", id);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapToUsuario(reader);
    }

    public async Task<Usuario?> CreateUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        DateTime fechaRegistro = usuario.FechaRegistro ?? DateTime.UtcNow;
        bool estaActivo = usuario.EstaActivo ?? true;

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO usuarios (nombre, apellido, email, fecha_registro, esta_activo)
                              VALUES (@nombre, @apellido, @email, @fechaRegistro, @estaActivo);
                              SELECT LAST_INSERT_ID();
                              """;
        command.Parameters.AddWithValue("@nombre", (object?)usuario.Nombre ?? DBNull.Value);
        command.Parameters.AddWithValue("@apellido", (object?)usuario.Apellido ?? DBNull.Value);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@fechaRegistro", fechaRegistro);
        command.Parameters.AddWithValue("@estaActivo", estaActivo);

        object? newIdRaw = await command.ExecuteScalarAsync(cancellationToken);
        if (newIdRaw is null || newIdRaw is DBNull)
        {
            return null;
        }

        int newId = Convert.ToInt32(newIdRaw);

        return new Usuario
        {
            Id = newId,
            Username = usuario.Username,
            PasswordHash = usuario.PasswordHash,
            Role = usuario.Role,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            FechaRegistro = fechaRegistro,
            EstaActivo = estaActivo
        };
    }

    public async Task<bool> UpdateUsuarioAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        bool estaActivo = usuario.EstaActivo ?? true;

        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              UPDATE usuarios
                              SET nombre = @nombre,
                                  apellido = @apellido,
                                  email = @email,
                                  esta_activo = @estaActivo
                              WHERE id = @id;
                              """;
        command.Parameters.AddWithValue("@id", usuario.Id);
        command.Parameters.AddWithValue("@nombre", (object?)usuario.Nombre ?? DBNull.Value);
        command.Parameters.AddWithValue("@apellido", (object?)usuario.Apellido ?? DBNull.Value);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@estaActivo", estaActivo);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteUsuarioAsync(int id, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              DELETE FROM usuarios
                              WHERE id = @id;
                              """;
        command.Parameters.AddWithValue("@id", id);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        StringBuilder sql = new("SELECT COUNT(1) FROM usuarios WHERE LOWER(email) = LOWER(@email)");
        command.Parameters.AddWithValue("@email", email);

        if (excludeId.HasValue)
        {
            sql.Append(" AND id <> @excludeId");
            command.Parameters.AddWithValue("@excludeId", excludeId.Value);
        }

        command.CommandText = sql.ToString();

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        int count = Convert.ToInt32(result);

        return count > 0;
    }

    public async Task<Usuario?> GetUsuarioByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              SELECT id, username, password_hash, role, nombre, apellido, email, fecha_registro, esta_activo
                              FROM usuarios
                              WHERE LOWER(username) = LOWER(@username)
                              LIMIT 1;
                              """;
        command.Parameters.AddWithValue("@username", username);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapToUsuario(reader);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        StringBuilder sql = new("SELECT COUNT(1) FROM usuarios WHERE LOWER(username) = LOWER(@username)");
        command.Parameters.AddWithValue("@username", username);

        if (excludeId.HasValue)
        {
            sql.Append(" AND id <> @excludeId");
            command.Parameters.AddWithValue("@excludeId", excludeId.Value);
        }

        command.CommandText = sql.ToString();

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        int count = Convert.ToInt32(result);

        return count > 0;
    }

    private static Usuario MapToUsuario(DbDataReader reader)
    {
        int idOrdinal = reader.GetOrdinal("id");
        int usernameOrdinal = reader.GetOrdinal("username");
        int passwordHashOrdinal = reader.GetOrdinal("password_hash");
        int roleOrdinal = reader.GetOrdinal("role");
        int nombreOrdinal = reader.GetOrdinal("nombre");
        int apellidoOrdinal = reader.GetOrdinal("apellido");
        int emailOrdinal = reader.GetOrdinal("email");
        int fechaRegistroOrdinal = reader.GetOrdinal("fecha_registro");
        int estaActivoOrdinal = reader.GetOrdinal("esta_activo");

        return new Usuario
        {
            Id = reader.GetInt32(idOrdinal),
            Username = reader.IsDBNull(usernameOrdinal) ? null : reader.GetString(usernameOrdinal),
            PasswordHash = reader.IsDBNull(passwordHashOrdinal) ? null : reader.GetString(passwordHashOrdinal),
            Role = reader.IsDBNull(roleOrdinal) ? null : reader.GetString(roleOrdinal),
            Nombre = reader.IsDBNull(nombreOrdinal) ? null : reader.GetString(nombreOrdinal),
            Apellido = reader.IsDBNull(apellidoOrdinal) ? null : reader.GetString(apellidoOrdinal),
            Email = reader.GetString(emailOrdinal),
            FechaRegistro = reader.IsDBNull(fechaRegistroOrdinal) ? null : reader.GetDateTime(fechaRegistroOrdinal),
            EstaActivo = reader.IsDBNull(estaActivoOrdinal) ? null : reader.GetBoolean(estaActivoOrdinal)
        };
    }
}
