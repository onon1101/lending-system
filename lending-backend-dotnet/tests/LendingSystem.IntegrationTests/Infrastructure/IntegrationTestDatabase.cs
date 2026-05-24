using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace LendingSystem.IntegrationTests.Infrastructure;

internal static class IntegrationTestDatabase
{
    public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.IntegrationTests.json", optional: false)
        .AddEnvironmentVariables()
        .Build();

    public static string ConnectionString => BuildConnectionString(Configuration);

    public static LendingDbContext CreateDbContext()
    {
        if (IntegrationTestTransaction.HasCurrent)
        {
            return IntegrationTestTransaction.CreateCurrentDbContext();
        }

        var options = new DbContextOptionsBuilder<LendingDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new LendingDbContext(options);
    }

    public static async Task UpdateSchemaAsync(CancellationToken cancellationToken = default)
    {
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync(cancellationToken);
        await RepairSchemaDriftAsync(db, cancellationToken);
    }

    private static async Task RepairSchemaDriftAsync(
        LendingDbContext db,
        CancellationToken cancellationToken = default)
    {
        await db.Database.ExecuteSqlRawAsync(
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
                IF NOT EXISTS (
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

                IF NOT EXISTS (
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
