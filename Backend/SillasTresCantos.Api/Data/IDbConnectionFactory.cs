using MySql.Data.MySqlClient;

namespace SillasTresCantos.Api.Data;

public interface IDbConnectionFactory
{
    MySqlConnection CreateConnection();
    Task<MySqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
