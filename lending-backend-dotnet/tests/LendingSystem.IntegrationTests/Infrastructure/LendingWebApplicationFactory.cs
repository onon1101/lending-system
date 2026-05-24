using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

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
    }
}
