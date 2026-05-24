using Xunit;

namespace LendingSystem.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private IntegrationTestTransaction? _transaction;

    protected IntegrationTestBase(IntegrationTestCollectionFixture fixture)
    {
        Factory = fixture.Factory;
    }

    protected LendingWebApplicationFactory Factory { get; }

    public async Task InitializeAsync()
    {
        _transaction = await IntegrationTestTransaction.BeginAsync();
    }

    public async Task DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
        }
    }
}
