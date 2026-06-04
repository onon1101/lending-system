using System.Text.Json;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using LendingSystem.WebApi;
using LendingSystem.WebApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LendingSystem.IntegrationTest.Framework.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly bool _isWriteTest;
    private string _connectionString = null!;
    private LendingWebApplicationFactory _factory = null!;

    protected IntegrationTestBase()
    {
        _isWriteTest = DetermineIfWriteTest();
    }

    // 每個測試實例都擁有自己獨立的 HttpClient 工廠
    protected WebApplicationFactory<Program> Factory => _factory;

    public async Task InitializeAsync()
    {
        await IntegrationTestDatabase.GlobalInitializeAsync();

        _factory = new LendingWebApplicationFactory();

        if (_isWriteTest)
        {
            // 1. 讀寫測試：從 Pool 租一個專屬的獨立資料庫名稱
            _connectionString = IntegrationTestDatabase.RentWriteDatabase();
        }
        else
        {
            // 2. 純讀測試：共用唯讀資料庫
            _connectionString = IntegrationTestDatabase.ReadonlyConnectionString;
        }

        // 將連線字串注入到該 Web 工廠中
        _factory.OverrideConnectionString(_connectionString);
    }

    public async Task DisposeAsync()
    {
        if (_isWriteTest && !string.IsNullOrEmpty(_connectionString))
        {
            // 測試結束後，清空該獨立資料庫的資料，並歸還 Pool
            await IntegrationTestDatabase.ReturnAndResetWriteDatabaseAsync(_connectionString);
        }

        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }
    }

    protected LendingDbContext CreateDbContext()
    {
        return IntegrationTestDatabase.CreateDbContext(_connectionString);
    }

    protected async Task<ApiResponse<T>> ParseJsonAsync<T>(HttpResponseMessage response)
    {
        var stream = await response.Content.ReadAsStreamAsync();
        return JsonSerializer.Deserialize<ApiResponse<T>>(stream)!;
    }

    private bool DetermineIfWriteTest()
    {
        return Attribute.IsDefined(GetType(), typeof(WriteTestAttribute), inherit: true);
    }
}
