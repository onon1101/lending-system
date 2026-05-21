using System.Data;
using LendingSystem.SharedKernel.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace LendingSystem.SharedKernel.Infrastructure.Persistence;

public sealed class PostgresQueryConnectionFactory(IConfiguration configuration) : IQueryConnectionFactory
{
    public IDbConnection CreateConnection() => new NpgsqlConnection(BuildConnectionString(configuration));

    private static string BuildConnectionString(IConfiguration configuration)
    {
        var host = configuration["DB_HOST"] ?? configuration["Database:Host"] ?? "";
        var port = configuration["DB_PORT"] ?? configuration["Database:Port"] ?? "5432";
        var user = configuration["DB_USER"] ?? configuration["Database:User"] ?? "";
        var password = configuration["DB_PASSWORD"] ?? configuration["Database:Password"] ?? "";
        var database = configuration["DB_NAME"] ?? configuration["Database:Name"] ?? "";

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = int.TryParse(port, out var parsedPort) ? parsedPort : 5432,
            Username = user,
            Password = password,
            Database = database,
            SslMode = SslMode.Disable,
            Pooling = true,
            MaxPoolSize = 25
        };

        return builder.ConnectionString;
    }
}
