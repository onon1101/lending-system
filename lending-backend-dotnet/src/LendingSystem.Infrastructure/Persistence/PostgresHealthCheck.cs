using LendingSystem.Application.System;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.Infrastructure.Persistence;

public sealed class PostgresHealthCheck(LendingDbContext db) : IDatabaseHealthCheck
{
    public async Task<string?> GetErrorAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await db.Database.CanConnectAsync(cancellationToken) ? null : "Database connection failed";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}
