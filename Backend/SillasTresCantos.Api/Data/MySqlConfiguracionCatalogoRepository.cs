using System.Data.Common;
using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public class MySqlConfiguracionCatalogoRepository : IConfiguracionCatalogoRepository
{
    private const int ConfiguracionId = 1;
    private readonly IDbConnectionFactory _connectionFactory;

    public MySqlConfiguracionCatalogoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ConfiguracionCatalogo> GetConfiguracionAsync(CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureConfiguracionAsync(connection, cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              SELECT usar_filtro_tabs, fecha_actualizacion
                              FROM configuracion_catalogo
                              WHERE id = @id
                              LIMIT 1;
                              """;
        command.Parameters.AddWithValue("@id", ConfiguracionId);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return DefaultConfiguracion();
        }

        return MapToConfiguracion(reader);
    }

    public async Task<ConfiguracionCatalogo> UpdateConfiguracionAsync(
        ConfiguracionCatalogo configuracion,
        CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureConfiguracionAsync(connection, cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO configuracion_catalogo (id, usar_filtro_tabs, fecha_actualizacion)
                              VALUES (@id, @usarFiltroTabs, @fechaActualizacion)
                              ON DUPLICATE KEY UPDATE
                                usar_filtro_tabs = VALUES(usar_filtro_tabs),
                                fecha_actualizacion = VALUES(fecha_actualizacion);
                              """;
        command.Parameters.AddWithValue("@id", ConfiguracionId);
        command.Parameters.AddWithValue("@usarFiltroTabs", configuracion.UsarFiltroTabs);
        command.Parameters.AddWithValue("@fechaActualizacion", configuracion.FechaActualizacion);

        await command.ExecuteNonQueryAsync(cancellationToken);
        return configuracion;
    }

    private static async Task EnsureConfiguracionAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        await using MySqlCommand createCommand = connection.CreateCommand();
        createCommand.CommandText = """
                                    CREATE TABLE IF NOT EXISTS configuracion_catalogo (
                                      id TINYINT PRIMARY KEY,
                                      usar_filtro_tabs BOOLEAN NOT NULL DEFAULT FALSE,
                                      fecha_actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                                    );
                                    """;
        await createCommand.ExecuteNonQueryAsync(cancellationToken);

        await using MySqlCommand seedCommand = connection.CreateCommand();
        seedCommand.CommandText = """
                                  INSERT IGNORE INTO configuracion_catalogo (id, usar_filtro_tabs, fecha_actualizacion)
                                  VALUES (@id, TRUE, CURRENT_TIMESTAMP);
                                  """;
        seedCommand.Parameters.AddWithValue("@id", ConfiguracionId);
        await seedCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static ConfiguracionCatalogo DefaultConfiguracion() =>
        new()
        {
            UsarFiltroTabs = true,
            FechaActualizacion = DateTime.UtcNow
        };

    private static ConfiguracionCatalogo MapToConfiguracion(DbDataReader reader)
    {
        int usarFiltroTabsOrdinal = reader.GetOrdinal("usar_filtro_tabs");
        int fechaActualizacionOrdinal = reader.GetOrdinal("fecha_actualizacion");

        return new ConfiguracionCatalogo
        {
            UsarFiltroTabs = reader.GetBoolean(usarFiltroTabsOrdinal),
            FechaActualizacion = reader.GetDateTime(fechaActualizacionOrdinal)
        };
    }
}
