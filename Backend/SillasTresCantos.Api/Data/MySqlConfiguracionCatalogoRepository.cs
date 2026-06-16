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
                              SELECT usar_filtro_tabs, mostrar_precios, mostrar_stock, fecha_actualizacion
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
                              INSERT INTO configuracion_catalogo (
                                id,
                                usar_filtro_tabs,
                                mostrar_precios,
                                mostrar_stock,
                                fecha_actualizacion
                              )
                              VALUES (
                                @id,
                                @usarFiltroTabs,
                                @mostrarPrecios,
                                @mostrarStock,
                                @fechaActualizacion
                              )
                              ON DUPLICATE KEY UPDATE
                                usar_filtro_tabs = VALUES(usar_filtro_tabs),
                                mostrar_precios = VALUES(mostrar_precios),
                                mostrar_stock = VALUES(mostrar_stock),
                                fecha_actualizacion = VALUES(fecha_actualizacion);
                              """;
        command.Parameters.AddWithValue("@id", ConfiguracionId);
        command.Parameters.AddWithValue("@usarFiltroTabs", configuracion.UsarFiltroTabs);
        command.Parameters.AddWithValue("@mostrarPrecios", configuracion.MostrarPrecios);
        command.Parameters.AddWithValue("@mostrarStock", configuracion.MostrarStock);
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
                                      mostrar_precios BOOLEAN NOT NULL DEFAULT FALSE,
                                      mostrar_stock BOOLEAN NOT NULL DEFAULT FALSE,
                                      fecha_actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                                    );
                                    """;
        await createCommand.ExecuteNonQueryAsync(cancellationToken);

        await EnsureColumnAsync(
            connection,
            "mostrar_precios",
            "mostrar_precios BOOLEAN NOT NULL DEFAULT FALSE AFTER usar_filtro_tabs",
            cancellationToken);
        await EnsureColumnAsync(
            connection,
            "mostrar_stock",
            "mostrar_stock BOOLEAN NOT NULL DEFAULT FALSE AFTER mostrar_precios",
            cancellationToken);

        await using MySqlCommand seedCommand = connection.CreateCommand();
        seedCommand.CommandText = """
                                  INSERT IGNORE INTO configuracion_catalogo (
                                    id,
                                    usar_filtro_tabs,
                                    mostrar_precios,
                                    mostrar_stock,
                                    fecha_actualizacion
                                  )
                                  VALUES (@id, TRUE, FALSE, FALSE, CURRENT_TIMESTAMP);
                                  """;
        seedCommand.Parameters.AddWithValue("@id", ConfiguracionId);
        await seedCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task EnsureColumnAsync(
        MySqlConnection connection,
        string columnName,
        string columnDefinition,
        CancellationToken cancellationToken)
    {
        await using MySqlCommand checkCommand = connection.CreateCommand();
        checkCommand.CommandText = """
                                   SELECT COUNT(*)
                                   FROM information_schema.COLUMNS
                                   WHERE TABLE_SCHEMA = DATABASE()
                                     AND TABLE_NAME = 'configuracion_catalogo'
                                     AND COLUMN_NAME = @columnName;
                                   """;
        checkCommand.Parameters.AddWithValue("@columnName", columnName);

        object? existingColumnCount = await checkCommand.ExecuteScalarAsync(cancellationToken);
        if (Convert.ToInt64(existingColumnCount ?? 0) > 0)
        {
            return;
        }

        await using MySqlCommand alterCommand = connection.CreateCommand();
        alterCommand.CommandText = $"ALTER TABLE configuracion_catalogo ADD COLUMN {columnDefinition};";
        await alterCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static ConfiguracionCatalogo DefaultConfiguracion() =>
        new()
        {
            UsarFiltroTabs = true,
            MostrarPrecios = false,
            MostrarStock = false,
            FechaActualizacion = DateTime.UtcNow
        };

    private static ConfiguracionCatalogo MapToConfiguracion(DbDataReader reader)
    {
        int usarFiltroTabsOrdinal = reader.GetOrdinal("usar_filtro_tabs");
        int mostrarPreciosOrdinal = reader.GetOrdinal("mostrar_precios");
        int mostrarStockOrdinal = reader.GetOrdinal("mostrar_stock");
        int fechaActualizacionOrdinal = reader.GetOrdinal("fecha_actualizacion");

        return new ConfiguracionCatalogo
        {
            UsarFiltroTabs = reader.GetBoolean(usarFiltroTabsOrdinal),
            MostrarPrecios = reader.GetBoolean(mostrarPreciosOrdinal),
            MostrarStock = reader.GetBoolean(mostrarStockOrdinal),
            FechaActualizacion = reader.GetDateTime(fechaActualizacionOrdinal)
        };
    }
}
