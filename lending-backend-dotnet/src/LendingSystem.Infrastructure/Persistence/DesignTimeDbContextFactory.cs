using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace LendingSystem.Infrastructure.Persistence;

public sealed class DesignTimeLendingDbContextFactory : IDesignTimeDbContextFactory<LendingDbContext>
{
    public LendingDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "LendingSystem.WebApi"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = BuildPostgresConnectionString(configuration);

        var optionsBuilder = new DbContextOptionsBuilder<LendingDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new LendingDbContext(optionsBuilder.Options);
    }

    private static string BuildPostgresConnectionString(IConfiguration configuration)
    {
        var host = configuration["DB_HOST"] ?? configuration["Database:Host"] ?? "localhost";
        var port = configuration["DB_PORT"] ?? configuration["Database:Port"] ?? "5432";
        var user = configuration["DB_USER"] ?? configuration["Database:User"] ?? "postgres";
        var password = configuration["DB_PASSWORD"] ?? configuration["Database:Password"] ?? "postgres";
        var database = configuration["DB_NAME"] ?? configuration["Database:Name"] ?? "postgres";

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
