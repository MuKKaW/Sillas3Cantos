using System.Data.Common;
using System.Text;
using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public class MySqlMarcaRepository : IMarcaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MySqlMarcaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Marca>> GetMarcasAsync(int idMarca, string nombre, bool orderAscent, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        StringBuilder sql = new("SELECT id, nombre, descripcion, pais_origen, anio_fundacion, es_visible, fecha_creacion, fecha_actualizacion FROM marcas");
        List<string> whereClauses = [];

        if (idMarca > 0)
        {
            whereClauses.Add("id = @idMarca");
            command.Parameters.AddWithValue("@idMarca", idMarca);
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

        List<Marca> marcas = [];
        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            marcas.Add(MapToMarca(reader));
        }

        return marcas;
    }

    public async Task<Marca?> GetMarcaByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              SELECT id, nombre, descripcion, pais_origen, anio_fundacion, es_visible, fecha_creacion, fecha_actualizacion
                              FROM marcas
                              WHERE id = @id
                              LIMIT 1;
                              """;
        command.Parameters.AddWithValue("@id", id);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapToMarca(reader);
    }

    public async Task<Marca?> CreateMarcaAsync(Marca marca, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO marcas (nombre, descripcion, pais_origen, anio_fundacion, es_visible, fecha_creacion, fecha_actualizacion)
                              VALUES (@nombre, @descripcion, @paisOrigen, @anioFundacion, @esVisible, @fechaCreacion, @fechaActualizacion);
                              SELECT LAST_INSERT_ID();
                              """;
        command.Parameters.AddWithValue("@nombre", marca.Nombre);
        command.Parameters.AddWithValue("@descripcion", (object?)marca.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("@paisOrigen", (object?)marca.PaisOrigen ?? DBNull.Value);
        command.Parameters.AddWithValue("@anioFundacion", (object?)marca.AnioFundacion ?? DBNull.Value);
        command.Parameters.AddWithValue("@esVisible", marca.EsVisible);
        command.Parameters.AddWithValue("@fechaCreacion", marca.FechaCreacion);
        command.Parameters.AddWithValue("@fechaActualizacion", (object?)marca.FechaActualizacion ?? DBNull.Value);

        object? newIdRaw = await command.ExecuteScalarAsync(cancellationToken);
        if (newIdRaw is null || newIdRaw is DBNull)
        {
            return null;
        }

        int newId = Convert.ToInt32(newIdRaw);

        return new Marca
        {
            Id = newId,
            Nombre = marca.Nombre,
            Descripcion = marca.Descripcion,
            PaisOrigen = marca.PaisOrigen,
            AnioFundacion = marca.AnioFundacion,
            EsVisible = marca.EsVisible,
            FechaCreacion = marca.FechaCreacion,
            FechaActualizacion = marca.FechaActualizacion
        };
    }

    public async Task<bool> UpdateMarcaAsync(Marca marca, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              UPDATE marcas
                              SET nombre = @nombre,
                                  descripcion = @descripcion,
                                  pais_origen = @paisOrigen,
                                  anio_fundacion = @anioFundacion,
                                  es_visible = @esVisible,
                                  fecha_actualizacion = @fechaActualizacion
                              WHERE id = @id;
                              """;
        command.Parameters.AddWithValue("@id", marca.Id);
        command.Parameters.AddWithValue("@nombre", marca.Nombre);
        command.Parameters.AddWithValue("@descripcion", (object?)marca.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("@paisOrigen", (object?)marca.PaisOrigen ?? DBNull.Value);
        command.Parameters.AddWithValue("@anioFundacion", (object?)marca.AnioFundacion ?? DBNull.Value);
        command.Parameters.AddWithValue("@esVisible", marca.EsVisible);
        command.Parameters.AddWithValue("@fechaActualizacion", (object?)marca.FechaActualizacion ?? DBNull.Value);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteMarcaAsync(int id, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              DELETE FROM marcas
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

        StringBuilder sql = new("SELECT COUNT(1) FROM marcas WHERE LOWER(nombre) = LOWER(@nombre)");
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

    private static Marca MapToMarca(DbDataReader reader)
    {
        int idOrdinal = reader.GetOrdinal("id");
        int nombreOrdinal = reader.GetOrdinal("nombre");
        int descripcionOrdinal = reader.GetOrdinal("descripcion");
        int paisOrigenOrdinal = reader.GetOrdinal("pais_origen");
        int anioFundacionOrdinal = reader.GetOrdinal("anio_fundacion");
        int esVisibleOrdinal = reader.GetOrdinal("es_visible");
        int fechaCreacionOrdinal = reader.GetOrdinal("fecha_creacion");
        int fechaActualizacionOrdinal = reader.GetOrdinal("fecha_actualizacion");

        return new Marca
        {
            Id = reader.GetInt32(idOrdinal),
            Nombre = reader.GetString(nombreOrdinal),
            Descripcion = reader.IsDBNull(descripcionOrdinal) ? null : reader.GetString(descripcionOrdinal),
            PaisOrigen = reader.IsDBNull(paisOrigenOrdinal) ? null : reader.GetString(paisOrigenOrdinal),
            AnioFundacion = reader.IsDBNull(anioFundacionOrdinal) ? null : reader.GetInt32(anioFundacionOrdinal),
            EsVisible = reader.GetBoolean(esVisibleOrdinal),
            FechaCreacion = reader.GetDateTime(fechaCreacionOrdinal),
            FechaActualizacion = reader.IsDBNull(fechaActualizacionOrdinal) ? null : reader.GetDateTime(fechaActualizacionOrdinal)
        };
    }
}
