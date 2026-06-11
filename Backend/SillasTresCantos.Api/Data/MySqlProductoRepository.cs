using System.Data.Common;
using System.Text;
using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Models;

namespace SillasTresCantos.Api.Data;

public class MySqlProductoRepository : IProductoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MySqlProductoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<Producto>> GetProductosAsync(int idProducto, string nombre, int categoriaId, int marcaId, int creadoPorUsuarioId, bool orderAscent, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        StringBuilder sql = new("SELECT id, nombre, descripcion, precio, stock, categoria_id, marca_id, creado_por_usuario_id, es_visible, imagen_url, imagen_public_id, imagen_resource_type, fecha_creacion, fecha_actualizacion FROM productos");
        List<string> whereClauses = [];

        if (idProducto > 0)
        {
            whereClauses.Add("id = @idProducto");
            command.Parameters.AddWithValue("@idProducto", idProducto);
        }

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            whereClauses.Add("nombre LIKE @nombre");
            command.Parameters.AddWithValue("@nombre", $"%{nombre}%");
        }

        if (categoriaId > 0)
        {
            whereClauses.Add("categoria_id = @categoriaId");
            command.Parameters.AddWithValue("@categoriaId", categoriaId);
        }

        if (marcaId > 0)
        {
            whereClauses.Add("marca_id = @marcaId");
            command.Parameters.AddWithValue("@marcaId", marcaId);
        }

        if (creadoPorUsuarioId > 0)
        {
            whereClauses.Add("creado_por_usuario_id = @creadoPorUsuarioId");
            command.Parameters.AddWithValue("@creadoPorUsuarioId", creadoPorUsuarioId);
        }

        if (whereClauses.Count > 0)
        {
            sql.Append(" WHERE ").Append(string.Join(" AND ", whereClauses));
        }

        sql.Append(orderAscent
            ? " ORDER BY COALESCE(nombre, '') ASC"
            : " ORDER BY COALESCE(nombre, '') DESC");

        command.CommandText = sql.ToString();

        List<Producto> productos = [];
        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            productos.Add(MapToProducto(reader));
        }

        return productos;
    }

    public async Task<Producto?> GetProductoByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              SELECT id, nombre, descripcion, precio, stock, categoria_id, marca_id, creado_por_usuario_id, es_visible, imagen_url, imagen_public_id, imagen_resource_type, fecha_creacion, fecha_actualizacion
                              FROM productos
                              WHERE id = @id
                              LIMIT 1;
                              """;
        command.Parameters.AddWithValue("@id", id);

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapToProducto(reader);
    }

    public async Task<Producto?> CreateProductoAsync(Producto producto, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              INSERT INTO productos (nombre, descripcion, precio, stock, categoria_id, marca_id, creado_por_usuario_id, es_visible, imagen_url, imagen_public_id, imagen_resource_type, fecha_creacion, fecha_actualizacion)
                              VALUES (@nombre, @descripcion, @precio, @stock, @categoriaId, @marcaId, @creadoPorUsuarioId, @esVisible, @imagenUrl, @imagenPublicId, @imagenResourceType, @fechaCreacion, @fechaActualizacion);
                              SELECT LAST_INSERT_ID();
                              """;
        command.Parameters.AddWithValue("@nombre", producto.Nombre);
        command.Parameters.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(producto.Descripcion) ? DBNull.Value : producto.Descripcion);
        command.Parameters.AddWithValue("@precio", producto.Precio);
        command.Parameters.AddWithValue("@stock", producto.Stock);
        command.Parameters.AddWithValue("@categoriaId", producto.CategoriaId);
        command.Parameters.AddWithValue("@marcaId", producto.MarcaId);
        command.Parameters.AddWithValue("@creadoPorUsuarioId", (object?)producto.CreadoPorUsuarioId ?? DBNull.Value);
        command.Parameters.AddWithValue("@esVisible", producto.EsVisible);
        command.Parameters.AddWithValue("@imagenUrl", (object?)producto.ImagenUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@imagenPublicId", (object?)producto.ImagenPublicId ?? DBNull.Value);
        command.Parameters.AddWithValue("@imagenResourceType", (object?)producto.ImagenResourceType ?? DBNull.Value);
        command.Parameters.AddWithValue("@fechaCreacion", producto.FechaCreacion);
        command.Parameters.AddWithValue("@fechaActualizacion", (object?)producto.FechaActualizacion ?? DBNull.Value);

        object? newIdRaw = await command.ExecuteScalarAsync(cancellationToken);
        if (newIdRaw is null || newIdRaw is DBNull)
        {
            return null;
        }

        int newId = Convert.ToInt32(newIdRaw);

        return new Producto
        {
            Id = newId,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            Precio = producto.Precio,
            Stock = producto.Stock,
            CategoriaId = producto.CategoriaId,
            MarcaId = producto.MarcaId,
            CreadoPorUsuarioId = producto.CreadoPorUsuarioId,
            EsVisible = producto.EsVisible,
            ImagenUrl = producto.ImagenUrl,
            ImagenPublicId = producto.ImagenPublicId,
            ImagenResourceType = producto.ImagenResourceType,
            FechaCreacion = producto.FechaCreacion,
            FechaActualizacion = producto.FechaActualizacion
        };
    }

    public async Task<bool> UpdateProductoAsync(Producto producto, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              UPDATE productos
                              SET nombre = @nombre,
                                  descripcion = @descripcion,
                                  precio = @precio,
                                  stock = @stock,
                                  categoria_id = @categoriaId,
                                  marca_id = @marcaId,
                                  creado_por_usuario_id = @creadoPorUsuarioId,
                                  es_visible = @esVisible,
                                  imagen_url = @imagenUrl,
                                  imagen_public_id = @imagenPublicId,
                                  imagen_resource_type = @imagenResourceType,
                                  fecha_actualizacion = @fechaActualizacion
                              WHERE id = @id;
                              """;
        command.Parameters.AddWithValue("@id", producto.Id);
        command.Parameters.AddWithValue("@nombre", producto.Nombre);
        command.Parameters.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(producto.Descripcion) ? DBNull.Value : producto.Descripcion);
        command.Parameters.AddWithValue("@precio", producto.Precio);
        command.Parameters.AddWithValue("@stock", producto.Stock);
        command.Parameters.AddWithValue("@categoriaId", producto.CategoriaId);
        command.Parameters.AddWithValue("@marcaId", producto.MarcaId);
        command.Parameters.AddWithValue("@creadoPorUsuarioId", (object?)producto.CreadoPorUsuarioId ?? DBNull.Value);
        command.Parameters.AddWithValue("@esVisible", producto.EsVisible);
        command.Parameters.AddWithValue("@imagenUrl", (object?)producto.ImagenUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@imagenPublicId", (object?)producto.ImagenPublicId ?? DBNull.Value);
        command.Parameters.AddWithValue("@imagenResourceType", (object?)producto.ImagenResourceType ?? DBNull.Value);
        command.Parameters.AddWithValue("@fechaActualizacion", (object?)producto.FechaActualizacion ?? DBNull.Value);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteProductoAsync(int id, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = """
                              DELETE FROM productos
                              WHERE id = @id;
                              """;
        command.Parameters.AddWithValue("@id", id);

        int affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    public async Task<bool> CategoriaExistsAsync(int categoriaId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = "SELECT COUNT(1) FROM categorias WHERE id = @categoriaId";
        command.Parameters.AddWithValue("@categoriaId", categoriaId);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        int count = Convert.ToInt32(result);

        return count > 0;
    }

    public async Task<bool> MarcaExistsAsync(int marcaId, CancellationToken cancellationToken = default)
    {
        await using MySqlConnection connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using MySqlCommand command = connection.CreateCommand();

        command.CommandText = "SELECT COUNT(1) FROM marcas WHERE id = @marcaId";
        command.Parameters.AddWithValue("@marcaId", marcaId);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        int count = Convert.ToInt32(result);

        return count > 0;
    }

    private static Producto MapToProducto(DbDataReader reader)
    {
        int idOrdinal = reader.GetOrdinal("id");
        int nombreOrdinal = reader.GetOrdinal("nombre");
        int descripcionOrdinal = reader.GetOrdinal("descripcion");
        int precioOrdinal = reader.GetOrdinal("precio");
        int stockOrdinal = reader.GetOrdinal("stock");
        int categoriaIdOrdinal = reader.GetOrdinal("categoria_id");
        int marcaIdOrdinal = reader.GetOrdinal("marca_id");
        int creadoPorUsuarioIdOrdinal = reader.GetOrdinal("creado_por_usuario_id");
        int esVisibleOrdinal = reader.GetOrdinal("es_visible");
        int imagenUrlOrdinal = reader.GetOrdinal("imagen_url");
        int imagenPublicIdOrdinal = reader.GetOrdinal("imagen_public_id");
        int imagenResourceTypeOrdinal = reader.GetOrdinal("imagen_resource_type");
        int fechaCreacionOrdinal = reader.GetOrdinal("fecha_creacion");
        int fechaActualizacionOrdinal = reader.GetOrdinal("fecha_actualizacion");

        return new Producto
        {
            Id = reader.GetInt32(idOrdinal),
            Nombre = reader.GetString(nombreOrdinal),
            Descripcion = reader.IsDBNull(descripcionOrdinal) ? null : reader.GetString(descripcionOrdinal),
            Precio = reader.GetDecimal(precioOrdinal),
            Stock = reader.GetInt32(stockOrdinal),
            CategoriaId = reader.GetInt32(categoriaIdOrdinal),
            MarcaId = reader.GetInt32(marcaIdOrdinal),
            CreadoPorUsuarioId = reader.IsDBNull(creadoPorUsuarioIdOrdinal) ? null : reader.GetInt32(creadoPorUsuarioIdOrdinal),
            EsVisible = reader.GetBoolean(esVisibleOrdinal),
            ImagenUrl = reader.IsDBNull(imagenUrlOrdinal) ? null : reader.GetString(imagenUrlOrdinal),
            ImagenPublicId = reader.IsDBNull(imagenPublicIdOrdinal) ? null : reader.GetString(imagenPublicIdOrdinal),
            ImagenResourceType = reader.IsDBNull(imagenResourceTypeOrdinal) ? null : reader.GetString(imagenResourceTypeOrdinal),
            FechaCreacion = reader.GetDateTime(fechaCreacionOrdinal),
            FechaActualizacion = reader.IsDBNull(fechaActualizacionOrdinal) ? null : reader.GetDateTime(fechaActualizacionOrdinal)
        };
    }
}
