using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.Auth.Application.Auth;
using LendingSystem.Auth.Domain.ValueObjects;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.IntegrationTests.Infrastructure;

public sealed class LendingWebApplicationFactory : WebApplicationFactory<Program>
{
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
            services.RemoveAll<DbContextOptions<LendingDbContext>>();
            services.RemoveAll<LendingDbContext>();
            services.RemoveAll<IQueryConnectionFactory>();
            services.RemoveAll<IObjectStorage>();
            services.RemoveAll<IGoogleOAuth2Acl>();

            services.AddScoped(_ => IntegrationTestTransaction.HasCurrent
                ? IntegrationTestTransaction.CreateCurrentDbContext()
                : IntegrationTestDatabase.CreateDbContext());
            services.AddScoped<IQueryConnectionFactory, IntegrationTestTransaction.QueryConnectionFactory>();
            services.AddSingleton<IObjectStorage, FakeObjectStorage>();
            services.AddSingleton<IGoogleOAuth2Acl, FakeGoogleOAuth2Acl>();
        });
    }

    private sealed class FakeObjectStorage : IObjectStorage
    {
        public Task<StoredObject> UploadItemImageAsync(Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken) =>
            Task.FromResult(new StoredObject($"https://storage.test/images/{fileName}", fileName));

        public Task<StoredObject> UploadItemVideoAsync(Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken) =>
            Task.FromResult(new StoredObject($"https://storage.test/videos/{fileName}", fileName));

        public Task DeleteObjectAsync(string objectName, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeGoogleOAuth2Acl : IGoogleOAuth2Acl
    {
        public Task<Result<ExternalLoginIdentity>> TranslateAsync(string idToken, CancellationToken cancellationToken) =>
            Task.FromResult(Result<ExternalLoginIdentity>.Success(
                new ExternalLoginIdentity(AuthProvider.Google, "google-user-1", "googleuser@example.com", "googleuser")));
    }
}
