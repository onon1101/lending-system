using LendingSystem.SharedKernel.Application.System;
using LendingSystem.WebApi.Modules.Definitions;
using LendingSystem.WebApi.Modules.Npgsql;
using LendingSystem.WebApi.Options;

namespace LendingSystem.WebApi.Modules.HealthCheck;

public sealed class HealthCheckModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var databaseOptions = configuration
            .GetRequiredSection(DatabaseOptions.SettingsName)
            .Get<DatabaseOptions>()
            ?? throw new InvalidOperationException("DatabaseOptions formatting Error.");

        var connectionString = BuildPostgresConnectionString.Get(databaseOptions);

        services
            .AddHealthChecks()
            .AddNpgSql(connectionString);

        services.AddScoped<SystemStatusService>();

        return services;
    }
    public override IApplicationBuilder UseApplication(WebApplication app)
    {
        app.MapGet(
                "/health",
                (SystemStatusService systemStatus) =>
                    Results.Ok(systemStatus.GetHealth()))
            .AllowAnonymous();

        app.MapGet(
                "/status",
                async (
                    SystemStatusService systemStatus,
                    CancellationToken cancellationToken) =>
                {
                    var status = await systemStatus.GetStatusAsync(cancellationToken);

                    return status.Status == "ok"
                        ? Results.Ok(status)
                        : Results.Json(
                            status,
                            statusCode: StatusCodes.Status503ServiceUnavailable);
                })
            .AllowAnonymous();

        return app;
    }
}
