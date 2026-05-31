namespace LendingSystem.IntegrationTest.Framework.Abstractions;

using LendingSystem.SharedKernel.Infrastructure.Persistence;

public abstract class InitializeData
{
    public virtual string TableName => string.Empty;
    public virtual int Order => 0;
    public abstract Task InsertBulkAsync(LendingDbContext db, CancellationToken cancellationToken = default);
}
