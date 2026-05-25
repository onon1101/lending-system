using System.Text.Json;
using LendingSystem.WebApi.Models;
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

    protected async Task<ApiResponse<T>> ParseJsonAsync<T>(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        var body = JsonSerializer.Deserialize<ApiResponse<T>>(stream)!;
        return body;
    }

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
