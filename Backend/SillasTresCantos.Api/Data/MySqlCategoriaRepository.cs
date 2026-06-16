using System.Data.Common;
using System.Text;
using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public class MySqlCategoriaRepository : ICategoriaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MySqlCategoriaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Categoria>> GetCategoriasAsync(int idCategoria, string nombre, bool orderAscent, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        StringBuilder sql = new("SELECT id, nombre, descripcion, orden_visual, es_visible, fecha_creacion, fecha_actualizacion FROM categorias");
        List<string> whereClauses = [];

        if (idCategoria > 0)
        {
            whereClauses.Add("id = @idCategoria");
            command.Parameters.AddWithValue("@idCategoria", idCategoria);
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
            ? " ORDER BY orden_visual ASC, COALESCE(nombre, '') ASC"
            : " ORDER BY orden_visual DESC, COALESCE(nombre, '') DESC");

        command.CommandText = sql.ToString();

        List<Categoria> categorias = [];
        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            categorias.Add(MapToCategoria(reader));
        }

        return categorias;
    }

    public async Task<Categoria?> GetCategoriaByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              SELECT id, nombre, descripcion, orden_visual, es_visible, fecha_creacion, fecha_actualizacion
                              FROM categorias
                              WHERE id = @id
                              LIMIT 1;
                              """;
        command.Parameters.AddWithValue("@id", id);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapToCategoria(reader);
    }

    public async Task<Categoria?> CreateCategoriaAsync(Categoria categoria, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO categorias (nombre, descripcion, orden_visual, es_visible, fecha_creacion, fecha_actualizacion)
                              VALUES (@nombre, @descripcion, @ordenVisual, @esVisible, @fechaCreacion, @fechaActualizacion);
                              SELECT LAST_INSERT_ID();
                              """;
        command.Parameters.AddWithValue("@nombre", categoria.Nombre);
        command.Parameters.AddWithValue("@descripcion", (object?)categoria.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("@ordenVisual", categoria.OrdenVisual);
        command.Parameters.AddWithValue("@esVisible", categoria.EsVisible);
        command.Parameters.AddWithValue("@fechaCreacion", categoria.FechaCreacion);
        command.Parameters.AddWithValue("@fechaActualizacion", (object?)categoria.FechaActualizacion ?? DBNull.Value);

        object? newIdRaw = await command.ExecuteScalarAsync(cancellationToken);
        if (newIdRaw is null || newIdRaw is DBNull)
        {
            return null;
        }

        int newId = Convert.ToInt32(newIdRaw);

        return new Categoria
        {
            Id = newId,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            OrdenVisual = categoria.OrdenVisual,
            EsVisible = categoria.EsVisible,
            FechaCreacion = categoria.FechaCreacion,
            FechaActualizacion = categoria.FechaActualizacion
        };
    }

    public async Task<bool> UpdateCategoriaAsync(Categoria categoria, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              UPDATE categorias
                              SET nombre = @nombre,
                                  descripcion = @descripcion,
                                  orden_visual = @ordenVisual,
                                  es_visible = @esVisible,
                                  fecha_actualizacion = @fechaActualizacion
                              WHERE id = @id;
                              """;
        command.Parameters.AddWithValue("@id", categoria.Id);
        command.Parameters.AddWithValue("@nombre", categoria.Nombre);
        command.Parameters.AddWithValue("@descripcion", (object?)categoria.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("@ordenVisual", categoria.OrdenVisual);
        command.Parameters.AddWithValue("@esVisible", categoria.EsVisible);
        command.Parameters.AddWithValue("@fechaActualizacion", (object?)categoria.FechaActualizacion ?? DBNull.Value);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteCategoriaAsync(int id, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              DELETE FROM categorias
                              WHERE id = @id;
                              """;
        command.Parameters.AddWithValue("@id", id);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> ExistsByNombreAsync(string nombre, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        StringBuilder sql = new("SELECT COUNT(1) FROM categorias WHERE LOWER(nombre) = LOWER(@nombre)");
        command.Parameters.AddWithValue("@nombre", nombre);

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

    private static Categoria MapToCategoria(DbDataReader reader)
    {
        int idOrdinal = reader.GetOrdinal("id");
        int nombreOrdinal = reader.GetOrdinal("nombre");
        int descripcionOrdinal = reader.GetOrdinal("descripcion");
        int ordenVisualOrdinal = reader.GetOrdinal("orden_visual");
        int esVisibleOrdinal = reader.GetOrdinal("es_visible");
        int fechaCreacionOrdinal = reader.GetOrdinal("fecha_creacion");
        int fechaActualizacionOrdinal = reader.GetOrdinal("fecha_actualizacion");

        return new Categoria
        {
            Id = reader.GetInt32(idOrdinal),
            Nombre = reader.GetString(nombreOrdinal),
            Descripcion = reader.IsDBNull(descripcionOrdinal) ? null : reader.GetString(descripcionOrdinal),
            OrdenVisual = reader.GetInt32(ordenVisualOrdinal),
            EsVisible = reader.GetBoolean(esVisibleOrdinal),
            FechaCreacion = reader.GetDateTime(fechaCreacionOrdinal),
            FechaActualizacion = reader.IsDBNull(fechaActualizacionOrdinal) ? null : reader.GetDateTime(fechaActualizacionOrdinal)
        };
    }
}
