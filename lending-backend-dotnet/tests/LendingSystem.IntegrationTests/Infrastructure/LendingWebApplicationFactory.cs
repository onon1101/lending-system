using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LendingSystem.SharedKernel.Application.Abstractions;
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
                ["APP_PORT"] = "0"
            });
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<LendingDbContext>>();
            services.RemoveAll<LendingDbContext>();
            services.RemoveAll<IQueryConnectionFactory>();

            services.AddScoped(_ => IntegrationTestTransaction.HasCurrent
                ? IntegrationTestTransaction.CreateCurrentDbContext()
                : IntegrationTestDatabase.CreateDbContext());
            services.AddScoped<IQueryConnectionFactory, IntegrationTestTransaction.QueryConnectionFactory>();
        });
    }
}
