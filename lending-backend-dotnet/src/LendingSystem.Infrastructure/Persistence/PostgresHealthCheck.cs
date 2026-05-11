using LendingSystem.Application.System;
using Npgsql;

namespace LendingSystem.Infrastructure.Persistence;

public sealed class PostgresHealthCheck(NpgsqlDataSource dataSource) : IDatabaseHealthCheck
{
    public async Task<string?> GetErrorAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var command = dataSource.CreateCommand("select 1");
            await command.ExecuteScalarAsync(cancellationToken);
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
