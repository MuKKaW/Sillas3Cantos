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

        StringBuilder sql = new("SELECT id, nombre FROM marcas");
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
                              SELECT id, nombre
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
                              INSERT INTO marcas (nombre)
                              VALUES (@nombre);
                              SELECT LAST_INSERT_ID();
                              """;
        command.Parameters.AddWithValue("@nombre", marca.Nombre);

        object? newIdRaw = await command.ExecuteScalarAsync(cancellationToken);
        if (newIdRaw is null || newIdRaw is DBNull)
        {
            return null;
        }

        int newId = Convert.ToInt32(newIdRaw);

        return new Marca
        {
            Id = newId,
            Nombre = marca.Nombre
        };
    }

    public async Task<bool> UpdateMarcaAsync(Marca marca, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              UPDATE marcas
                              SET nombre = @nombre
                              WHERE id = @id;
                              """;
        command.Parameters.AddWithValue("@id", marca.Id);
        command.Parameters.AddWithValue("@nombre", marca.Nombre);

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

        return new Marca
        {
            Id = reader.GetInt32(idOrdinal),
            Nombre = reader.GetString(nombreOrdinal)
        };
    }
}
