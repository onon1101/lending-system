using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LendingSystem.IntegrationTests.Infrastructure;

internal sealed class IntegrationTestTransaction : IAsyncDisposable
{
    private static readonly AsyncLocal<IntegrationTestTransaction?> Current = new();

    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;

    private IntegrationTestTransaction(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public static bool HasCurrent => Current.Value is not null;

    public static async Task<IntegrationTestTransaction> BeginAsync(CancellationToken cancellationToken = default)
    {
        if (Current.Value is not null)
        {
            throw new InvalidOperationException("An integration test transaction is already active.");
        }

        var connection = new NpgsqlConnection(IntegrationTestDatabase.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var current = new IntegrationTestTransaction(connection, transaction);
        Current.Value = current;
        return current;
    }

    public static LendingDbContext CreateCurrentDbContext()
    {
        var current = Current.Value;
        if (current is null)
        {
            throw new InvalidOperationException("No integration test transaction is active.");
        }

        var options = new DbContextOptionsBuilder<LendingDbContext>()
            .UseNpgsql(current._connection)
            .Options;

        var db = new LendingDbContext(options);
        db.Database.UseTransaction(current._transaction);
        return db;
    }

    public static IDbConnection CreateCurrentQueryConnection()
    {
        var current = Current.Value;
        if (current is null)
        {
            throw new InvalidOperationException("No integration test transaction is active.");
        }

        return new NonDisposingDbConnection(current._connection);
    }

    public async ValueTask DisposeAsync()
    {
        if (ReferenceEquals(Current.Value, this))
        {
            Current.Value = null;
        }

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private sealed class NonDisposingDbConnection(DbConnection inner) : DbConnection
    {
        [AllowNull]
        public override string ConnectionString
        {
            get => inner.ConnectionString;
            set => inner.ConnectionString = value;
        }

        public override string Database => inner.Database;
        public override string DataSource => inner.DataSource;
        public override string ServerVersion => inner.ServerVersion;
        public override ConnectionState State => inner.State;

        public override void ChangeDatabase(string databaseName) => inner.ChangeDatabase(databaseName);
        public override void Close() { }
        public override void Open() => inner.Open();
        public override Task OpenAsync(CancellationToken cancellationToken) => inner.OpenAsync(cancellationToken);

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) =>
            throw new NotSupportedException("Integration test query connections share the active test transaction.");

        protected override DbCommand CreateDbCommand() => inner.CreateCommand();

        protected override void Dispose(bool disposing)
        {
        }

        public override ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    public sealed class QueryConnectionFactory : IQueryConnectionFactory
    {
        public IDbConnection CreateConnection() => HasCurrent
            ? CreateCurrentQueryConnection()
            : new NpgsqlConnection(IntegrationTestDatabase.ConnectionString);
    }
}
