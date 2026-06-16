using System.Data.Common;
using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public class MySqlPermisosCatalogoRepository : IPermisosCatalogoRepository
{
    private const string UserRole = "User";
    private readonly IDbConnectionFactory _connectionFactory;

    public MySqlPermisosCatalogoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CatalogoPermisos> GetPermisosAsync(string rol, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureTableAsync(connection, cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              SELECT rol,
                                     productos_crear,
                                     productos_modificar,
                                     productos_eliminar,
                                     categorias_crear,
                                     categorias_modificar,
                                     categorias_eliminar,
                                     marcas_crear,
                                     marcas_modificar,
                                     marcas_eliminar,
                                     soluciones_crear,
                                     soluciones_modificar,
                                     soluciones_eliminar,
                                     fecha_actualizacion
                              FROM permisos_rol_catalogo
                              WHERE rol = @rol
                              LIMIT 1;
                              """;
        command.Parameters.AddWithValue("@rol", NormalizeRole(rol));

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return DefaultPermisos(NormalizeRole(rol));
        }

        return MapToPermisos(reader);
    }

    public async Task<CatalogoPermisos> UpdatePermisosAsync(
        CatalogoPermisos permisos,
        CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await EnsureTableAsync(connection, cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO permisos_rol_catalogo (
                                rol,
                                productos_crear,
                                productos_modificar,
                                productos_eliminar,
                                categorias_crear,
                                categorias_modificar,
                                categorias_eliminar,
                                marcas_crear,
                                marcas_modificar,
                                marcas_eliminar,
                                soluciones_crear,
                                soluciones_modificar,
                                soluciones_eliminar,
                                fecha_actualizacion
                              )
                              VALUES (
                                @rol,
                                @productosCrear,
                                @productosModificar,
                                @productosEliminar,
                                @categoriasCrear,
                                @categoriasModificar,
                                @categoriasEliminar,
                                @marcasCrear,
                                @marcasModificar,
                                @marcasEliminar,
                                @solucionesCrear,
                                @solucionesModificar,
                                @solucionesEliminar,
                                @fechaActualizacion
                              )
                              ON DUPLICATE KEY UPDATE
                                productos_crear = VALUES(productos_crear),
                                productos_modificar = VALUES(productos_modificar),
                                productos_eliminar = VALUES(productos_eliminar),
                                categorias_crear = VALUES(categorias_crear),
                                categorias_modificar = VALUES(categorias_modificar),
                                categorias_eliminar = VALUES(categorias_eliminar),
                                marcas_crear = VALUES(marcas_crear),
                                marcas_modificar = VALUES(marcas_modificar),
                                marcas_eliminar = VALUES(marcas_eliminar),
                                soluciones_crear = VALUES(soluciones_crear),
                                soluciones_modificar = VALUES(soluciones_modificar),
                                soluciones_eliminar = VALUES(soluciones_eliminar),
                                fecha_actualizacion = VALUES(fecha_actualizacion);
                              """;
        command.Parameters.AddWithValue("@rol", NormalizeRole(permisos.Rol));
        command.Parameters.AddWithValue("@productosCrear", permisos.ProductosCrear);
        command.Parameters.AddWithValue("@productosModificar", permisos.ProductosModificar);
        command.Parameters.AddWithValue("@productosEliminar", permisos.ProductosEliminar);
        command.Parameters.AddWithValue("@categoriasCrear", permisos.CategoriasCrear);
        command.Parameters.AddWithValue("@categoriasModificar", permisos.CategoriasModificar);
        command.Parameters.AddWithValue("@categoriasEliminar", permisos.CategoriasEliminar);
        command.Parameters.AddWithValue("@marcasCrear", permisos.MarcasCrear);
        command.Parameters.AddWithValue("@marcasModificar", permisos.MarcasModificar);
        command.Parameters.AddWithValue("@marcasEliminar", permisos.MarcasEliminar);
        command.Parameters.AddWithValue("@solucionesCrear", permisos.SolucionesCrear);
        command.Parameters.AddWithValue("@solucionesModificar", permisos.SolucionesModificar);
        command.Parameters.AddWithValue("@solucionesEliminar", permisos.SolucionesEliminar);
        command.Parameters.AddWithValue("@fechaActualizacion", permisos.FechaActualizacion);

        await command.ExecuteNonQueryAsync(cancellationToken);
        permisos.Rol = NormalizeRole(permisos.Rol);
        return permisos;
    }

    private static async Task EnsureTableAsync(MySqlConnection connection, CancellationToken cancellationToken)
    {
        await using MySqlCommand createCommand = connection.CreateCommand();
        createCommand.CommandText = """
                                    CREATE TABLE IF NOT EXISTS permisos_rol_catalogo (
                                      rol VARCHAR(30) PRIMARY KEY,
                                      productos_crear BOOLEAN NOT NULL DEFAULT TRUE,
                                      productos_modificar BOOLEAN NOT NULL DEFAULT TRUE,
                                      productos_eliminar BOOLEAN NOT NULL DEFAULT TRUE,
                                      categorias_crear BOOLEAN NOT NULL DEFAULT TRUE,
                                      categorias_modificar BOOLEAN NOT NULL DEFAULT TRUE,
                                      categorias_eliminar BOOLEAN NOT NULL DEFAULT TRUE,
                                      marcas_crear BOOLEAN NOT NULL DEFAULT TRUE,
                                      marcas_modificar BOOLEAN NOT NULL DEFAULT TRUE,
                                      marcas_eliminar BOOLEAN NOT NULL DEFAULT TRUE,
                                      soluciones_crear BOOLEAN NOT NULL DEFAULT TRUE,
                                      soluciones_modificar BOOLEAN NOT NULL DEFAULT TRUE,
                                      soluciones_eliminar BOOLEAN NOT NULL DEFAULT TRUE,
                                      fecha_actualizacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                                    );
                                    """;
        await createCommand.ExecuteNonQueryAsync(cancellationToken);

        await EnsureColumnAsync(
            connection,
            "soluciones_crear",
            "soluciones_crear BOOLEAN NOT NULL DEFAULT TRUE AFTER marcas_eliminar",
            cancellationToken);
        await EnsureColumnAsync(
            connection,
            "soluciones_modificar",
            "soluciones_modificar BOOLEAN NOT NULL DEFAULT TRUE AFTER soluciones_crear",
            cancellationToken);
        await EnsureColumnAsync(
            connection,
            "soluciones_eliminar",
            "soluciones_eliminar BOOLEAN NOT NULL DEFAULT TRUE AFTER soluciones_modificar",
            cancellationToken);

        await using MySqlCommand seedCommand = connection.CreateCommand();
        seedCommand.CommandText = """
                                  INSERT IGNORE INTO permisos_rol_catalogo (
                                    rol,
                                    productos_crear,
                                    productos_modificar,
                                    productos_eliminar,
                                    categorias_crear,
                                    categorias_modificar,
                                    categorias_eliminar,
                                    marcas_crear,
                                    marcas_modificar,
                                    marcas_eliminar,
                                    soluciones_crear,
                                    soluciones_modificar,
                                    soluciones_eliminar,
                                    fecha_actualizacion
                                  )
                                  VALUES (@rol, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, TRUE, CURRENT_TIMESTAMP);
                                  """;
        seedCommand.Parameters.AddWithValue("@rol", UserRole);
        await seedCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static CatalogoPermisos DefaultPermisos(string rol) =>
        new()
        {
            Rol = NormalizeRole(rol),
            ProductosCrear = true,
            ProductosModificar = true,
            ProductosEliminar = true,
            CategoriasCrear = true,
            CategoriasModificar = true,
            CategoriasEliminar = true,
            MarcasCrear = true,
            MarcasModificar = true,
            MarcasEliminar = true,
            SolucionesCrear = true,
            SolucionesModificar = true,
            SolucionesEliminar = true,
            FechaActualizacion = DateTime.UtcNow
        };

    private static CatalogoPermisos MapToPermisos(DbDataReader reader)
    {
        int rolOrdinal = reader.GetOrdinal("rol");
        int productosCrearOrdinal = reader.GetOrdinal("productos_crear");
        int productosModificarOrdinal = reader.GetOrdinal("productos_modificar");
        int productosEliminarOrdinal = reader.GetOrdinal("productos_eliminar");
        int categoriasCrearOrdinal = reader.GetOrdinal("categorias_crear");
        int categoriasModificarOrdinal = reader.GetOrdinal("categorias_modificar");
        int categoriasEliminarOrdinal = reader.GetOrdinal("categorias_eliminar");
        int marcasCrearOrdinal = reader.GetOrdinal("marcas_crear");
        int marcasModificarOrdinal = reader.GetOrdinal("marcas_modificar");
        int marcasEliminarOrdinal = reader.GetOrdinal("marcas_eliminar");
        int solucionesCrearOrdinal = reader.GetOrdinal("soluciones_crear");
        int solucionesModificarOrdinal = reader.GetOrdinal("soluciones_modificar");
        int solucionesEliminarOrdinal = reader.GetOrdinal("soluciones_eliminar");
        int fechaActualizacionOrdinal = reader.GetOrdinal("fecha_actualizacion");

        return new CatalogoPermisos
        {
            Rol = reader.GetString(rolOrdinal),
            ProductosCrear = reader.GetBoolean(productosCrearOrdinal),
            ProductosModificar = reader.GetBoolean(productosModificarOrdinal),
            ProductosEliminar = reader.GetBoolean(productosEliminarOrdinal),
            CategoriasCrear = reader.GetBoolean(categoriasCrearOrdinal),
            CategoriasModificar = reader.GetBoolean(categoriasModificarOrdinal),
            CategoriasEliminar = reader.GetBoolean(categoriasEliminarOrdinal),
            MarcasCrear = reader.GetBoolean(marcasCrearOrdinal),
            MarcasModificar = reader.GetBoolean(marcasModificarOrdinal),
            MarcasEliminar = reader.GetBoolean(marcasEliminarOrdinal),
            SolucionesCrear = reader.GetBoolean(solucionesCrearOrdinal),
            SolucionesModificar = reader.GetBoolean(solucionesModificarOrdinal),
            SolucionesEliminar = reader.GetBoolean(solucionesEliminarOrdinal),
            FechaActualizacion = reader.GetDateTime(fechaActualizacionOrdinal)
        };
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
                                     AND TABLE_NAME = 'permisos_rol_catalogo'
                                     AND COLUMN_NAME = @columnName;
                                   """;
        checkCommand.Parameters.AddWithValue("@columnName", columnName);

        object? existingColumnCount = await checkCommand.ExecuteScalarAsync(cancellationToken);
        if (Convert.ToInt64(existingColumnCount ?? 0) > 0)
        {
            return;
        }

        await using MySqlCommand alterCommand = connection.CreateCommand();
        alterCommand.CommandText = $"ALTER TABLE permisos_rol_catalogo ADD COLUMN {columnDefinition};";
        try
        {
            await alterCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (MySqlException exception) when (exception.Number == 1060)
        {
            // Another request may have created the column between the check and the ALTER.
        }
    }

    private static string NormalizeRole(string? rol) =>
        string.IsNullOrWhiteSpace(rol) ? UserRole : rol.Trim();
}
