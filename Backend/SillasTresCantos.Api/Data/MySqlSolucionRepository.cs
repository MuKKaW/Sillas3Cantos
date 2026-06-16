using System.Data.Common;
using System.Text;
using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public class MySqlSolucionRepository : ISolucionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MySqlSolucionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Solucion>> GetSolucionesAsync(int idSolucion, string titulo, bool orderAscent, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureTableAsync(connection, cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        StringBuilder sql = new("SELECT id, titulo, texto, emoji, orden_visual, fecha_creacion, fecha_actualizacion FROM soluciones");
        List<string> whereClauses = [];

        if (idSolucion > 0)
        {
            whereClauses.Add("id = @idSolucion");
            command.Parameters.AddWithValue("@idSolucion", idSolucion);
        }

        if (!string.IsNullOrWhiteSpace(titulo))
        {
            whereClauses.Add("titulo LIKE @titulo");
            command.Parameters.AddWithValue("@titulo", $"%{titulo}%");
        }

        if (whereClauses.Count > 0)
        {
            sql.Append(" WHERE ").Append(string.Join(" AND ", whereClauses));
        }

        sql.Append(orderAscent
            ? " ORDER BY orden_visual ASC, COALESCE(titulo, '') ASC"
            : " ORDER BY orden_visual DESC, COALESCE(titulo, '') DESC");

        command.CommandText = sql.ToString();

        List<Solucion> soluciones = [];
        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            soluciones.Add(MapToSolucion(reader));
        }

        return soluciones;
    }

    public async Task<Solucion?> GetSolucionByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureTableAsync(connection, cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              SELECT id, titulo, texto, emoji, orden_visual, fecha_creacion, fecha_actualizacion
                              FROM soluciones
                              WHERE id = @id
                              LIMIT 1;
                              """;
        command.Parameters.AddWithValue("@id", id);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapToSolucion(reader);
    }

    public async Task<Solucion?> CreateSolucionAsync(Solucion solucion, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureTableAsync(connection, cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO soluciones (titulo, texto, emoji, orden_visual, fecha_creacion, fecha_actualizacion)
                              VALUES (@titulo, @texto, @emoji, @ordenVisual, @fechaCreacion, @fechaActualizacion);
                              SELECT LAST_INSERT_ID();
                              """;
        command.Parameters.AddWithValue("@titulo", solucion.Titulo);
        command.Parameters.AddWithValue("@texto", solucion.Texto);
        command.Parameters.AddWithValue("@emoji", solucion.Emoji);
        command.Parameters.AddWithValue("@ordenVisual", solucion.OrdenVisual);
        command.Parameters.AddWithValue("@fechaCreacion", solucion.FechaCreacion);
        command.Parameters.AddWithValue("@fechaActualizacion", (object?)solucion.FechaActualizacion ?? DBNull.Value);

        object? newIdRaw = await command.ExecuteScalarAsync(cancellationToken);
        if (newIdRaw is null || newIdRaw is DBNull)
        {
            return null;
        }

        int newId = Convert.ToInt32(newIdRaw);

        return new Solucion
        {
            Id = newId,
            Titulo = solucion.Titulo,
            Texto = solucion.Texto,
            Emoji = solucion.Emoji,
            OrdenVisual = solucion.OrdenVisual,
            FechaCreacion = solucion.FechaCreacion,
            FechaActualizacion = solucion.FechaActualizacion
        };
    }

    public async Task<bool> UpdateSolucionAsync(Solucion solucion, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureTableAsync(connection, cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              UPDATE soluciones
                              SET titulo = @titulo,
                                  texto = @texto,
                                  emoji = @emoji,
                                  orden_visual = @ordenVisual,
                                  fecha_actualizacion = @fechaActualizacion
                              WHERE id = @id;
                              """;
        command.Parameters.AddWithValue("@id", solucion.Id);
        command.Parameters.AddWithValue("@titulo", solucion.Titulo);
        command.Parameters.AddWithValue("@texto", solucion.Texto);
        command.Parameters.AddWithValue("@emoji", solucion.Emoji);
        command.Parameters.AddWithValue("@ordenVisual", solucion.OrdenVisual);
        command.Parameters.AddWithValue("@fechaActualizacion", (object?)solucion.FechaActualizacion ?? DBNull.Value);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteSolucionAsync(int id, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureTableAsync(connection, cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              DELETE FROM soluciones
                              WHERE id = @id;
                              """;
        command.Parameters.AddWithValue("@id", id);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> ExistsByTituloAsync(string titulo, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureTableAsync(connection, cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        StringBuilder sql = new("SELECT COUNT(1) FROM soluciones WHERE LOWER(titulo) = LOWER(@titulo)");
        command.Parameters.AddWithValue("@titulo", titulo);

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

    private static async Task EnsureTableAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        await using MySqlCommand createCommand = connection.CreateCommand();
        createCommand.CommandText = """
                                    CREATE TABLE IF NOT EXISTS soluciones (
                                      id INT AUTO_INCREMENT PRIMARY KEY,
                                      titulo VARCHAR(100) NOT NULL,
                                      texto VARCHAR(255) NOT NULL,
                                      emoji VARCHAR(32) NOT NULL,
                                      orden_visual INT NOT NULL DEFAULT 0,
                                      fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                                      fecha_actualizacion DATETIME NULL DEFAULT NULL,
                                      UNIQUE KEY uq_soluciones_titulo (titulo)
                                    );
                                    """;
        await createCommand.ExecuteNonQueryAsync(cancellationToken);

        await using MySqlCommand countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(1) FROM soluciones;";
        object? existingCountRaw = await countCommand.ExecuteScalarAsync(cancellationToken);
        if (Convert.ToInt32(existingCountRaw ?? 0) > 0)
        {
            return;
        }

        await using MySqlCommand seedCommand = connection.CreateCommand();
        seedCommand.CommandText = """
                                  INSERT INTO soluciones (titulo, texto, emoji, orden_visual, fecha_creacion)
                                  VALUES
                                    ('Sillas de ruedas', 'Venta y alquiler para movilidad diaria o temporal.', '♿', 1, CURRENT_TIMESTAMP),
                                    ('Camas articuladas', 'Descanso cómodo con soluciones geriátricas.', '▭', 2, CURRENT_TIMESTAMP),
                                    ('Scooters eléctricos', 'Autonomía sencilla para moverse cada día.', '⚡', 3, CURRENT_TIMESTAMP),
                                    ('Andadores', 'Apoyo estable, ligero y fácil de manejar.', '↗', 4, CURRENT_TIMESTAMP),
                                    ('Grúas de traslado', 'Ayuda segura para movilización en casa.', '⌁', 5, CURRENT_TIMESTAMP),
                                    ('Ortesis', 'Soportes técnicos para articulaciones y cuidado.', '+', 6, CURRENT_TIMESTAMP),
                                    ('Ayudas de baño', 'Seguridad y autonomía para el aseo diario.', '□', 7, CURRENT_TIMESTAMP),
                                    ('Plantillas y calzado', 'Adaptación y comodidad para pies delicados.', '✓', 8, CURRENT_TIMESTAMP);
                                  """;
        await seedCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static Solucion MapToSolucion(DbDataReader reader)
    {
        int idOrdinal = reader.GetOrdinal("id");
        int tituloOrdinal = reader.GetOrdinal("titulo");
        int textoOrdinal = reader.GetOrdinal("texto");
        int emojiOrdinal = reader.GetOrdinal("emoji");
        int ordenVisualOrdinal = reader.GetOrdinal("orden_visual");
        int fechaCreacionOrdinal = reader.GetOrdinal("fecha_creacion");
        int fechaActualizacionOrdinal = reader.GetOrdinal("fecha_actualizacion");

        return new Solucion
        {
            Id = reader.GetInt32(idOrdinal),
            Titulo = reader.GetString(tituloOrdinal),
            Texto = reader.GetString(textoOrdinal),
            Emoji = reader.GetString(emojiOrdinal),
            OrdenVisual = reader.GetInt32(ordenVisualOrdinal),
            FechaCreacion = reader.GetDateTime(fechaCreacionOrdinal),
            FechaActualizacion = reader.IsDBNull(fechaActualizacionOrdinal) ? null : reader.GetDateTime(fechaActualizacionOrdinal)
        };
    }
}
