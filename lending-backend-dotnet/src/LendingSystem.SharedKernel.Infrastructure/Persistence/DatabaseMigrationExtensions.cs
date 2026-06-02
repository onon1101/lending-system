using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace LendingSystem.SharedKernel.Infrastructure.Persistence;

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
        if (appliedMigrations.Count == 0 && await HasCurrentSchemaAsync(db))
        {
            await BaselineExistingSchemaAsync(db, migrations);
        }
        else if (appliedMigrations.Count == 0 && await HasLegacyInitialSchemaAsync(db))
        {
            await BaselineExistingSchemaAsync(db, migrations[0]);
        }

        await db.Database.EnsureMediaSchemaAsync();
        await db.Database.MigrateAsync();
        await db.Database.EnsureMediaSchemaAsync();
    }

    public static Task EnsureCurrentSchemaAsync(this LendingDbContext db, CancellationToken cancellationToken = default) =>
        db.Database.EnsureMediaSchemaAsync(cancellationToken);

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

    private static Task BaselineExistingSchemaAsync(LendingDbContext db, string initialMigration) =>
        BaselineExistingSchemaAsync(db, [initialMigration]);

    private static async Task BaselineExistingSchemaAsync(LendingDbContext db, IReadOnlyCollection<string> migrations)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL,
                "ProductVersion" character varying(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
            );
            """);

        foreach (var migration in migrations)
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                VALUES ({migration}, '10.0.4')
                ON CONFLICT ("MigrationId") DO NOTHING;
                """);
        }
    }

    private static async Task<bool> HasLegacyInitialSchemaAsync(LendingDbContext db)
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

    private static async Task<bool> HasCurrentSchemaAsync(LendingDbContext db)
    {
        const string sql = """
            SELECT
                EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = current_schema()
                      AND table_name = 'items'
                      AND column_name = 'item_id'
                )
                AND EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = current_schema()
                      AND table_name = 'users'
                      AND column_name = 'status'
                )
                AND EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = current_schema()
                      AND table_name = 'user_auth_identities'
                )
                AND EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = current_schema()
                      AND table_name = 'borrower_details'
                )
                AND EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = current_schema()
                      AND table_name = 'item_media'
                )
                AND EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = current_schema()
                      AND table_name = 'lending_media'
                );
            """;

        var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToBoolean(await command.ExecuteScalarAsync());
    }

    private static Task EnsureMediaSchemaAsync(
        this DatabaseFacade database,
        CancellationToken cancellationToken = default) =>
        database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "item_media" (
                "media_id" bigint NOT NULL PRIMARY KEY,
                "item_id" bigint NOT NULL,
                "type" character varying(20) NOT NULL,
                "url" text NOT NULL,
                "link" text NULL,
                "description" text NULL,
                "created_at" timestamp without time zone NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS "lending_media" (
                "media_id" bigint NOT NULL PRIMARY KEY,
                "order_id" bigint NOT NULL,
                "type" character varying(20) NOT NULL,
                "url" text NOT NULL,
                "link" text NULL,
                "description" text NULL,
                "created_at" timestamp without time zone NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE INDEX IF NOT EXISTS "idx_item_media_item_id" ON "item_media" ("item_id");
            CREATE INDEX IF NOT EXISTS "idx_lending_media_order_id" ON "lending_media" ("order_id");

            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM information_schema.tables
                    WHERE table_schema = current_schema()
                      AND table_name = 'items'
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM information_schema.table_constraints
                    WHERE constraint_schema = current_schema()
                      AND table_name = 'item_media'
                      AND constraint_name = 'fk_item_media_item'
                ) THEN
                    ALTER TABLE "item_media"
                    ADD CONSTRAINT "fk_item_media_item"
                    FOREIGN KEY ("item_id") REFERENCES "items" ("item_id")
                    ON DELETE CASCADE;
                END IF;

                IF EXISTS (
                    SELECT 1 FROM information_schema.tables
                    WHERE table_schema = current_schema()
                      AND table_name = 'orders'
                )
                AND NOT EXISTS (
                    SELECT 1
                    FROM information_schema.table_constraints
                    WHERE constraint_schema = current_schema()
                      AND table_name = 'lending_media'
                      AND constraint_name = 'fk_lending_media_order'
                ) THEN
                    ALTER TABLE "lending_media"
                    ADD CONSTRAINT "fk_lending_media_order"
                    FOREIGN KEY ("order_id") REFERENCES "orders" ("order_id")
                    ON DELETE CASCADE;
                END IF;
            END $$;
            """,
            cancellationToken);
}
