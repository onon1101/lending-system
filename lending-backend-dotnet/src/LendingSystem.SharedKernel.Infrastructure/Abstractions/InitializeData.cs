using LendingSystem.SharedKernel.Infrastructure.Persistence;

namespace LendingSystem.SharedKernel.Infrastructure.Abstractions;

public abstract class InitializeData
{
    public virtual string TableName => string.Empty;
    public virtual int Order => 0;
    public abstract Task InsertBulkAsync(LendingDbContext db, CancellationToken cancellationToken = default);
}
