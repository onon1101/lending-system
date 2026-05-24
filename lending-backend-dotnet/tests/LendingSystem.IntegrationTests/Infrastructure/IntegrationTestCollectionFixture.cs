using Xunit;

namespace LendingSystem.IntegrationTests.Infrastructure;

public sealed class IntegrationTestCollectionFixture : IAsyncLifetime
{
    private readonly DatabaseSchemaFixture _database = new();

    public LendingWebApplicationFactory Factory { get; } = new();

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();

        using var client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
        await Factory.DisposeAsync();
    }
}
