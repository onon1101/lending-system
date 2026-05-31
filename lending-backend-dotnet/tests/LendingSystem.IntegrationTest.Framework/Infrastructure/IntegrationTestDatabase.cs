using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;
using LendingSystem.IntegrationTest.Framework.Abstractions;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace LendingSystem.IntegrationTest.Framework.Infrastructure;

/// <summary>
/// 提供整合測試 Database 功能
/// </summary>
internal static class IntegrationTestDatabase
{
    /// <summary>
    /// For Writing Database Pool Size
    /// </summary>
    private const int WriteDatabasePoolSize = 8;

    /// <summary>
    /// lock
    /// </summary>
    private static readonly SemaphoreSlim InitializeLock = new(1, 1);

    /// <summary>
    /// 可使用的寫資料庫
    /// </summary>
    private static readonly ConcurrentQueue<string> AvailableWriteDbNames = new();

    /// <summary>
    /// 是否已經初始化
    /// </summary>
    private static bool _initialized;
    
    /// <summary>
    /// 初始化資料庫資料
    /// </summary>
    private static IReadOnlyCollection<InitializeData> _cachedInitialData = Array.Empty<InitializeData>();

    public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.IntegrationTests.json", optional: false)
        .AddEnvironmentVariables()
        .Build();

    /// <summary>
    /// 建構子
    /// </summary>
    static IntegrationTestDatabase()
    {
        foreach (var dbName in WriteDatabaseNames)
        {
            AvailableWriteDbNames.Enqueue(dbName);
        }
    }
    
    public static string ReadonlyConnectionString => BuildConnectionString(ReadonlyDatabaseName);
    private static string BaseDatabaseName => Configuration["DB_NAME"] ?? Configuration["Database:Name"] ?? "lending_test";
    private static string MaintenanceDatabaseName => Configuration["DB_MAINTENANCE_NAME"] ?? Configuration["Database:MaintenanceName"] ?? "postgres";
    private static string ReadonlyDatabaseName => $"{BaseDatabaseName}_readonly";
    
    private static IReadOnlyCollection<string> WriteDatabaseNames =>
        Enumerable.Range(1, WriteDatabasePoolSize)
            .Select(index => $"{BaseDatabaseName}_write_pool_{index}")
            .ToArray();
    
    public static LendingDbContext CreateDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<LendingDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new LendingDbContext(options);
    }
    
    public static async Task GlobalInitializeAsync(CancellationToken cancellationToken = default)
    {
        await InitializeLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized) return;

            _cachedInitialData = await GetInitialDataAsync();

            // 1. 初始化 Readonly 資料庫（含 Seed Data）
            await PrepareDatabaseAsync(ReadonlyConnectionString, _cachedInitialData, seedInitialData: true, cancellationToken);

            // 2. 初始化所有的 Write 獨立資料庫（不含 Seed Data，維持乾淨環境）
            foreach (var dbName in WriteDatabaseNames)
            {
                await PrepareDatabaseAsync(BuildConnectionString(dbName), _cachedInitialData, seedInitialData: false, cancellationToken);
            }

            _initialized = true;
        }
        finally
        {
            InitializeLock.Release();
        }
    }
    
    public static string RentWriteDatabase()
    {
        if (!AvailableWriteDbNames.TryDequeue(out var dbName))
        {
            throw new InvalidOperationException("測試資料庫連接池（Write DB Pool）已耗盡！請增加 Pool 大小或降低測試平行度。");
        }
        return BuildConnectionString(dbName);
    }
    
    public static async Task ReturnAndResetWriteDatabaseAsync(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.Database))
        {
            throw new InvalidOperationException("歸還的連線字串不包含正確的資料庫名稱。");
        }

        // 測試完畢歸還前，快速清空該獨立資料庫的所有資料，確保下一個測試拿到的是乾淨的
        await using var db = CreateDbContext(connectionString);
        await ResetDatabaseAsync(db, _cachedInitialData);

        AvailableWriteDbNames.Enqueue(builder.Database);
    }
    
    private static async Task PrepareDatabaseAsync(
        string connectionString,
        IReadOnlyCollection<InitializeData> initialData,
        bool seedInitialData,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseExistsAsync(connectionString, cancellationToken);

        await using var db = CreateDbContext(connectionString);
        await db.Database.MigrateAsync(cancellationToken);
        await db.EnsureCurrentSchemaAsync(cancellationToken);
        
        await ResetDatabaseAsync(db, initialData, cancellationToken);

        if (seedInitialData)
        {
            await InsertInitialDataAsync(db, initialData, cancellationToken);
        }
    }
    
    private static async Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        var targetBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = targetBuilder.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Integration test database name cannot be empty.");
        }

        var masterBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = MaintenanceDatabaseName,
            Pooling = false
        };

        await using var connection = new NpgsqlConnection(masterBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var existsCommand = connection.CreateCommand();
        existsCommand.CommandText = "select exists (select 1 from pg_database where datname = @databaseName);";
        existsCommand.Parameters.AddWithValue("databaseName", databaseName);

        var exists = (bool)(await existsCommand.ExecuteScalarAsync(cancellationToken) ?? false);
        if (exists) return;

        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = $"""create database "{databaseName.Replace("\"", "\"\"")}";""";
        await createCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<IReadOnlyCollection<InitializeData>> GetInitialDataAsync()
    {
        var seedBaseType = typeof(InitializeData);
        var seeds = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsAbstract: false, IsInterface: false } && seedBaseType.IsAssignableFrom(type))
            .Select(type => (InitializeData?)Activator.CreateInstance(type))
            .OfType<InitializeData>()
            .OrderBy(seed => seed.Order)
            .ToArray();

        return await Task.FromResult(seeds);
    }

    private static async Task ResetDatabaseAsync(LendingDbContext db, IReadOnlyCollection<InitializeData> initialData, CancellationToken cancellationToken = default)
    {
        if (!initialData.Any()) return;

        var tables = initialData.Select(item => item.TableName).ToImmutableArray();
        var sqlTables = new StringBuilder().AppendLine("truncate table");

        for (var i = 0; i < tables.Length; i++)
        {
            sqlTables.AppendLine(tables[i]);
            if (i < tables.Length - 1) sqlTables.AppendLine(",");
        }
        sqlTables.AppendLine("restart identity cascade;");

        await db.Database.ExecuteSqlRawAsync(sqlTables.ToString(), cancellationToken);
    }

    private static async Task InsertInitialDataAsync(LendingDbContext db, IReadOnlyCollection<InitializeData> initialData, CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
        foreach (var seed in initialData)
        {
            await seed.InsertBulkAsync(db, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }
        await tx.CommitAsync(cancellationToken);
    }

    private static string BuildConnectionString(string database)
    {
        var host = Configuration["DB_HOST"] ?? Configuration["Database:Host"] ?? "localhost";
        var port = Configuration["DB_PORT"] ?? Configuration["Database:Port"] ?? "5432";
        var user = Configuration["DB_USER"] ?? Configuration["Database:User"] ?? "postgres";
        var password = Configuration["DB_PASSWORD"] ?? Configuration["Database:Password"] ?? "postgres";

        return new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = int.TryParse(port, out var parsedPort) ? parsedPort : 5432,
            Username = user,
            Password = password,
            Database = database,
            SslMode = GetSslMode(),
            Pooling = true,
            MaxPoolSize = 30
        }.ConnectionString;
    }

    private static SslMode GetSslMode()
    {
        var value = Configuration["DB_SSL_MODE"] ?? Configuration["Database:SslMode"];
        return Enum.TryParse<SslMode>(value, ignoreCase: true, out var sslMode)
            ? sslMode
            : SslMode.Prefer;
    }
}
