using System.Data;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.Auth.Application.Auth;
using LendingSystem.Auth.Domain.ValueObjects;
using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace LendingSystem.IntegrationTest.Framework.Infrastructure;

public sealed class LendingWebApplicationFactory : WebApplicationFactory<Program>
{
    private string _currentConnectionString = IntegrationTestDatabase.ReadonlyConnectionString;

    // 動態設定目前該 WebApplicationFactory 應該連向哪一個資料庫
    internal void OverrideConnectionString(string connectionString)
    {
        _currentConnectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddConfiguration(IntegrationTestDatabase.Configuration);
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SECRET_KEY"] = "integration-test-secret-key-with-enough-length",
                ["APP_PORT"] = "0",
                ["Development:ExecutionContext:UserId"] = "1000",
                ["Development:ExecutionContext:Email"] = "owner@example.com"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // 移除原本專案中的 DbContext 註冊
            services.RemoveAll<DbContextOptions<LendingDbContext>>();
            services.RemoveAll<LendingDbContext>();
            services.RemoveAll<IQueryConnectionFactory>();
            services.RemoveAll<IGoogleOAuth2Acl>();
            services.RemoveAll<IObjectStorage>();

            // 重新註冊，使其永遠向 Scope 請求當前指定的隔離連線字串
            services.AddScoped(_ => IntegrationTestDatabase.CreateDbContext(_currentConnectionString));
            services.AddScoped<IQueryConnectionFactory>(_ => new TestQueryConnectionFactory(_currentConnectionString));
            services.AddSingleton<IGoogleOAuth2Acl, TestGoogleOAuth2Acl>();
            services.AddSingleton<IObjectStorage, TestObjectStorage>();
        });
    }

    private sealed class TestQueryConnectionFactory(string connectionString) : IQueryConnectionFactory
    {
        public IDbConnection CreateConnection() => new NpgsqlConnection(connectionString);
    }

    private sealed class TestGoogleOAuth2Acl : IGoogleOAuth2Acl
    {
        public Task<Result<ExternalLoginIdentity>> TranslateAsync(string idToken, CancellationToken cancellationToken)
        {
            if (idToken != "valid-google-token")
            {
                return Task.FromResult(Result<ExternalLoginIdentity>.Failure(AuthErrors.GoogleLoginFailed()));
            }

            return Task.FromResult(Result<ExternalLoginIdentity>.Success(
                new ExternalLoginIdentity(
                    AuthProvider.Google,
                    "test-google-user-id",
                    "google-login@example.com",
                    "Google Login User")));
        }
    }

    private sealed class TestObjectStorage : IObjectStorage
    {
        public Task<StoredObject> UploadItemImageAsync(
            Stream stream,
            long size,
            string fileName,
            string contentType,
            CancellationToken cancellationToken) =>
            Task.FromResult(new StoredObject($"https://storage.test/images/{fileName}", fileName));

        public Task<StoredObject> UploadItemVideoAsync(
            Stream stream,
            long size,
            string fileName,
            string contentType,
            CancellationToken cancellationToken) =>
            Task.FromResult(new StoredObject($"https://storage.test/videos/{fileName}", fileName));

        public Task DeleteObjectAsync(string objectName, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
