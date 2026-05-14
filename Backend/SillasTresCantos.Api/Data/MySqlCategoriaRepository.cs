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

        StringBuilder sql = new("SELECT id, nombre FROM categorias");
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
            ? " ORDER BY COALESCE(nombre, '') ASC"
            : " ORDER BY COALESCE(nombre, '') DESC");

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
                              SELECT id, nombre
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
                              INSERT INTO categorias (nombre)
                              VALUES (@nombre);
                              SELECT LAST_INSERT_ID();
                              """;
        command.Parameters.AddWithValue("@nombre", categoria.Nombre);

        object? newIdRaw = await command.ExecuteScalarAsync(cancellationToken);
        if (newIdRaw is null || newIdRaw is DBNull)
        {
            return null;
        }

        int newId = Convert.ToInt32(newIdRaw);

        return new Categoria
        {
            Id = newId,
            Nombre = categoria.Nombre
        };
    }

    public async Task<bool> UpdateCategoriaAsync(Categoria categoria, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              UPDATE categorias
                              SET nombre = @nombre
                              WHERE id = @id;
                              """;
        command.Parameters.AddWithValue("@id", categoria.Id);
        command.Parameters.AddWithValue("@nombre", categoria.Nombre);

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

        return new Categoria
        {
            Id = reader.GetInt32(idOrdinal),
            Nombre = reader.GetString(nombreOrdinal)
        };
    }
}
