using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LendingSystem.Infrastructure.Persistence;

public static class DatabaseMigrationExtensions
{
    public static async Task MigrateDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LendingDbContext>();

        var migrations = db.Database.GetMigrations().ToArray();
        if (migrations.Length == 0)
        {
            return;
        }

        var appliedMigrations = await GetAppliedMigrationsAsync(db);
        if (appliedMigrations.Count == 0 && await HasScannedLegacySchemaAsync(db))
        {
            await BaselineExistingSchemaAsync(db, migrations[0]);
        }

        await db.Database.MigrateAsync();
    }

    private static async Task<IReadOnlyCollection<string>> GetAppliedMigrationsAsync(LendingDbContext db)
    {
        try
        {
            return (await db.Database.GetAppliedMigrationsAsync()).ToArray();
        }
        catch
        {
            return [];
        }
    }

    private static async Task<bool> HasScannedLegacySchemaAsync(LendingDbContext db)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_name IN ('users', 'items', 'orders', 'order_details', 'media');
            """;

        var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var tableCount = Convert.ToInt32(await command.ExecuteScalarAsync());
        return tableCount == 5;
    }

    private static async Task BaselineExistingSchemaAsync(LendingDbContext db, string initialMigration)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL,
                "ProductVersion" character varying(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            );
            """);

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ({initialMigration}, '10.0.4')
            ON CONFLICT ("MigrationId") DO NOTHING;
            """);
    }
}
