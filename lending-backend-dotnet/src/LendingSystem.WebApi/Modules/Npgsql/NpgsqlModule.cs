using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.Auth.Infrastructure.Persistence;
using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Application.Abstractions.Loans;
using LendingSystem.Lending.Infrastructure.Persistence;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.System;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using LendingSystem.WebApi.Modules.Definitions;
using LendingSystem.WebApi.Options;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.WebApi.Modules.Npgsql;

public sealed class NpgsqlModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var databaseOptions = configuration
            .GetRequiredSection(DatabaseOptions.SettingsName)
            .Get<DatabaseOptions>()
            ?? throw new InvalidOperationException(
                "DatabaseOptions formatting Error.");

        var connectionString =
            BuildPostgresConnectionString.Get(databaseOptions);

        services.AddDbContext<LendingDbContext>(
            options => options.UseNpgsql(connectionString));

        services.AddScoped<IUserCommandRepository, UserRepository>();
        services.AddScoped<IUserQueryRepository, UserRepository>();
        services.AddScoped<IItemCommandRepository, ItemRepository>();
        services.AddScoped<IItemQueryRepository, ItemRepository>();
        services.AddScoped<ILoanCommandRepository, LoanRepository>();
        services.AddScoped<ILoanQueryRepository, LoanRepository>();
        services.AddScoped<ILoanRequestItemReader, LoanRepository>();
        services.AddScoped<ILoanPrepareBorrowerDetailReference, LoanRepository>();
        services.AddScoped<ILoanRequestDecisionReader, LoanRepository>();
        services.AddScoped<IMediaCommandRepository, MediaRepository>();
        services.AddScoped<IQueryConnectionFactory, PostgresQueryConnectionFactory>();
        services.AddScoped<IDatabaseHealthCheck, PostgresHealthCheck>();

        return services;
    }
}
