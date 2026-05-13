using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using SillasTresCantos.Api.Configuration;

namespace SillasTresCantos.Api.Data;

public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(IConfiguration configuration, IOptions<DatabaseOptions> databaseOptions)
    {
        string connectionStringName = databaseOptions.Value.ConnectionStringName;
        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new InvalidOperationException("La configuracion Database:ConnectionStringName es obligatoria.");
        }

        _connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException($"No existe la cadena de conexion '{connectionStringName}'.");
    }

    public MySqlConnection CreateConnection() => new(_connectionString);

    public async Task<MySqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        MySqlConnection connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
