using LendingSystem.WebApi.Options;
using Npgsql;

namespace LendingSystem.WebApi.Modules.Npgsql;

public static class BuildPostgresConnectionString
{
    public static string Get(DatabaseOptions databaseOptions)
    {
        SslMode sslMode = databaseOptions.SslMode switch
        {
            "Disable" => SslMode.Disable,
            "Allow" => SslMode.Allow,
            "Prefer" => SslMode.Prefer,
            "Require" => SslMode.Require,
            "VerifyCA" => SslMode.VerifyCA,
            "VerifyFull" => SslMode.VerifyFull,
            _ => throw new InvalidOperationException("the database settings's sslMode mapping rule cannot be found.")
        };

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = databaseOptions.Host,
            Port = databaseOptions.Port,
            Username = databaseOptions.User,
            Password = databaseOptions.Password,
            Database = databaseOptions.Name,
            SslMode = sslMode,
            Pooling = databaseOptions.Pooling,
            MaxPoolSize = databaseOptions.MaxPoolSize,
        };

        return builder.ConnectionString;
    }
}
