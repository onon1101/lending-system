using LendingSystem.Application.Abstractions;
using LendingSystem.Application.Auth;
using LendingSystem.Application.Items;
using LendingSystem.Application.Loans;
using LendingSystem.Application.Media;
using LendingSystem.Application.System;
using LendingSystem.Infrastructure.Auth;
using LendingSystem.Infrastructure.Persistence;
using LendingSystem.Infrastructure.Storage;
using LendingSystem.Infrastructure.Time;
using LendingSystem.Infrastructure.Video;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Npgsql;

namespace LendingSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<ItemService>();
        services.AddScoped<LoanService>();
        services.AddScoped<MediaService>();
        services.AddScoped<SystemStatusService>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = BuildPostgresConnectionString(configuration);
        services.AddDbContext<LendingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<IDatabaseHealthCheck, PostgresHealthCheck>();

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IVideoDownloadClient, GrpcVideoDownloadClient>();

        services.AddSingleton<IMinioClient>(_ =>
        {
            var endpoint = configuration["MINIO_ENDPOINT"] ?? configuration["Minio:Endpoint"] ?? "";
            var accessKey = configuration["MINIO_ACCESS_KEY"] ?? configuration["Minio:AccessKey"] ?? "";
            var secretKey = configuration["MINIO_SECRET_KEY"] ?? configuration["Minio:SecretKey"] ?? "";
            return new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(false)
                .Build();
        });
        services.AddSingleton<IObjectStorage, MinioObjectStorage>();

        return services;
    }

    private static string BuildPostgresConnectionString(IConfiguration configuration)
    {
        var host = configuration["DB_HOST"] ?? configuration["Database:Host"] ?? "";
        var port = configuration["DB_PORT"] ?? configuration["Database:Port"] ?? "5432";
        var user = configuration["DB_USER"] ?? configuration["Database:User"] ?? "";
        var password = configuration["DB_PASSWORD"] ?? configuration["Database:Password"] ?? "";
        var database = configuration["DB_NAME"] ?? configuration["Database:Name"] ?? "";

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = int.TryParse(port, out var parsedPort) ? parsedPort : 5432,
            Username = user,
            Password = password,
            Database = database,
            SslMode = SslMode.Disable,
            Pooling = true,
            MaxPoolSize = 25
        };

        return builder.ConnectionString;
    }
}
