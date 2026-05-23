using System.Collections.Immutable;
using System.Text;
using LendingSystem.IntegrationTests.Abstractions;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LendingSystem.IntegrationTests.Infrastructure;

public sealed class DatabaseSchemaFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await IntegrationTestDatabase.UpdateSchemaAsync();

        await using var db = IntegrationTestDatabase.CreateDbContext();

        var initialDataTypes = await GetInitialDataAsync();
        
        await ResetDatabaseAsync(db, initialDataTypes);

        await InsertInitialDataAsync(db, initialDataTypes);
    }

    private static async Task<IReadOnlyCollection<InitializeData>>
        GetInitialDataAsync()
    {
        var seedBaseType = typeof(InitializeData);
        
        var seeds = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           seedBaseType.IsAssignableFrom(type))
            .Select(type => (InitializeData?)Activator.CreateInstance(type))
            .OfType<InitializeData>()
            .OrderBy(seed => seed.Order)
            .ThenBy(seed => seed.GetType().Name)
            .ToArray();

        return seeds;
    }

    private static async Task ResetDatabaseAsync(
        LendingDbContext db,
        IReadOnlyCollection<InitializeData>  initialData,
        CancellationToken cancellationToken = default)
    {
        var tables = initialData
            .Select(item => item.TableName)
            .ToImmutableArray();

        var sqlTables = new StringBuilder();
        sqlTables.AppendLine("truncate table");
        
        for (var i = 0; i < tables.Length; i++)
        {
            sqlTables.AppendLine(tables[i]);
            if (i < tables.Length - 1)
            {
                sqlTables.AppendLine(",");
            }
        }
        
        sqlTables.AppendLine("restart identity cascade;");
        
        await db.Database.ExecuteSqlRawAsync(
            sqlTables.ToString(),
            cancellationToken);
    }

    private static async Task InsertInitialDataAsync(
        LendingDbContext db,
        IReadOnlyCollection<InitializeData> initialData,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        foreach (var seed in initialData)
        {
            await seed.InsertBulkAsync(db, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
    }

    public Task DisposeAsync() => Task.CompletedTask;
}
