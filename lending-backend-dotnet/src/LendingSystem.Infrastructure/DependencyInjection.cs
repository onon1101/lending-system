using LendingSystem.Auth.ACL.Google;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.Auth.Application.Auth;
using LendingSystem.Lending.Application.Items;
using LendingSystem.Lending.Application.Loans;
using LendingSystem.Lending.Application.Media;
using LendingSystem.SharedKernel.Application.System;
using LendingSystem.Auth.Infrastructure.Auth;
using LendingSystem.Auth.Infrastructure.Persistence;
using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Infrastructure.Persistence;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using LendingSystem.Lending.Infrastructure.Storage;
using LendingSystem.SharedKernel.Infrastructure.Time;
using LendingSystem.Lending.Infrastructure.Video;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Npgsql;

namespace LendingSystem.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication()
        {
            services.AddScoped<AuthService>();
            services.AddScoped<ItemService>();
            services.AddScoped<LoanService>();
            services.AddScoped<MediaService>();
            services.AddScoped<SystemStatusService>();
            return services;
        }

        public IServiceCollection AddInfrastructure(IConfiguration configuration)
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
        services.AddSingleton<IGoogleOAuth2Acl, GoogleOAuth2Acl>();
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
