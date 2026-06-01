using System.Data.Common;
using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public class MySqlProductoArchivoRepository : IProductoArchivoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MySqlProductoArchivoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ProductoArchivo>> GetArchivosByProductoIdAsync(int productoId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              SELECT id, producto_id, subido_por_usuario_id, nombre_original, nombre_almacenado, ruta_relativa, content_type, tamano_bytes, fecha_subida
                              FROM producto_archivos
                              WHERE producto_id = @productoId
                              ORDER BY fecha_subida DESC;
                              """;
        command.Parameters.AddWithValue("@productoId", productoId);

        List<ProductoArchivo> archivos = [];
        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            archivos.Add(MapToProductoArchivo(reader));
        }

        return archivos;
    }

    public async Task<ProductoArchivo?> GetArchivoByIdAsync(int archivoId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              SELECT id, producto_id, subido_por_usuario_id, nombre_original, nombre_almacenado, ruta_relativa, content_type, tamano_bytes, fecha_subida
                              FROM producto_archivos
                              WHERE id = @archivoId
                              LIMIT 1;
                              """;
        command.Parameters.AddWithValue("@archivoId", archivoId);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapToProductoArchivo(reader);
    }

    public async Task<ProductoArchivo?> CreateArchivoAsync(ProductoArchivo archivo, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO producto_archivos (producto_id, subido_por_usuario_id, nombre_original, nombre_almacenado, ruta_relativa, content_type, tamano_bytes, fecha_subida)
                              VALUES (@productoId, @subidoPorUsuarioId, @nombreOriginal, @nombreAlmacenado, @rutaRelativa, @contentType, @tamanoBytes, @fechaSubida);
                              SELECT LAST_INSERT_ID();
                              """;
        command.Parameters.AddWithValue("@productoId", archivo.ProductoId);
        command.Parameters.AddWithValue("@subidoPorUsuarioId", (object?)archivo.SubidoPorUsuarioId ?? DBNull.Value);
        command.Parameters.AddWithValue("@nombreOriginal", archivo.NombreOriginal);
        command.Parameters.AddWithValue("@nombreAlmacenado", archivo.NombreAlmacenado);
        command.Parameters.AddWithValue("@rutaRelativa", archivo.RutaRelativa);
        command.Parameters.AddWithValue("@contentType", archivo.ContentType);
        command.Parameters.AddWithValue("@tamanoBytes", archivo.TamanoBytes);
        command.Parameters.AddWithValue("@fechaSubida", archivo.FechaSubida);

        object? newIdRaw = await command.ExecuteScalarAsync(cancellationToken);
        if (newIdRaw is null || newIdRaw is DBNull)
        {
            return null;
        }

        int newId = Convert.ToInt32(newIdRaw);

        return new ProductoArchivo
        {
            Id = newId,
            ProductoId = archivo.ProductoId,
            SubidoPorUsuarioId = archivo.SubidoPorUsuarioId,
            NombreOriginal = archivo.NombreOriginal,
            NombreAlmacenado = archivo.NombreAlmacenado,
            RutaRelativa = archivo.RutaRelativa,
            ContentType = archivo.ContentType,
            TamanoBytes = archivo.TamanoBytes,
            FechaSubida = archivo.FechaSubida
        };
    }

    public async Task<bool> DeleteArchivoAsync(int archivoId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              DELETE FROM producto_archivos
                              WHERE id = @archivoId;
                              """;
        command.Parameters.AddWithValue("@archivoId", archivoId);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    private static ProductoArchivo MapToProductoArchivo(DbDataReader reader)
    {
        int idOrdinal = reader.GetOrdinal("id");
        int productoIdOrdinal = reader.GetOrdinal("producto_id");
        int subidoPorUsuarioIdOrdinal = reader.GetOrdinal("subido_por_usuario_id");
        int nombreOriginalOrdinal = reader.GetOrdinal("nombre_original");
        int nombreAlmacenadoOrdinal = reader.GetOrdinal("nombre_almacenado");
        int rutaRelativaOrdinal = reader.GetOrdinal("ruta_relativa");
        int contentTypeOrdinal = reader.GetOrdinal("content_type");
        int tamanoBytesOrdinal = reader.GetOrdinal("tamano_bytes");
        int fechaSubidaOrdinal = reader.GetOrdinal("fecha_subida");

        return new ProductoArchivo
        {
            Id = reader.GetInt32(idOrdinal),
            ProductoId = reader.GetInt32(productoIdOrdinal),
            SubidoPorUsuarioId = reader.IsDBNull(subidoPorUsuarioIdOrdinal) ? null : reader.GetInt32(subidoPorUsuarioIdOrdinal),
            NombreOriginal = reader.GetString(nombreOriginalOrdinal),
            NombreAlmacenado = reader.GetString(nombreAlmacenadoOrdinal),
            RutaRelativa = reader.GetString(rutaRelativaOrdinal),
            ContentType = reader.GetString(contentTypeOrdinal),
            TamanoBytes = reader.GetInt64(tamanoBytesOrdinal),
            FechaSubida = reader.GetDateTime(fechaSubidaOrdinal)
        };
    }
}
