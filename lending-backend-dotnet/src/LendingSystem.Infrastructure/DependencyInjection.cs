using System.ComponentModel.DataAnnotations;
using System.Reflection;
using LendingSystem.Auth.ACL.Google;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.Auth.Application.Auth;
using LendingSystem.Auth.Application.Auth.PasskeyRegistrationOption;
using LendingSystem.Lending.Application.Items;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Application.System;
using LendingSystem.Auth.Infrastructure.Auth;
using LendingSystem.Auth.Infrastructure.Persistence;
using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Infrastructure.Persistence;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using LendingSystem.Lending.Infrastructure.Storage;
using LendingSystem.Infrastructure.Messaging;
using LendingSystem.SharedKernel.Infrastructure.Messaging;
using LendingSystem.SharedKernel.Infrastructure.Time;
using LendingSystem.Lending.Infrastructure.Video;
using FluentValidation;
using LendingSystem.Auth.Application.Auth.Login;
using LendingSystem.Lending.Application.Items.GetAllItems;
using MediatR;
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
            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssemblies(
                    typeof(LoginCommand).Assembly,
                    typeof(GetAllItemsQuery).Assembly,
                    typeof(ItemRepository).Assembly,
                    typeof(SystemStatusService).Assembly);
            });
            services.AddValidatorsFromAssemblies(
                typeof(LoginCommand).Assembly,
                typeof(GetAllItemsQuery).Assembly,
                typeof(SystemStatusService).Assembly);
            services.AddScoped<SystemStatusService>();
            services.AddSingleton<IUserAccessChecker, UserAccessChecker>();
            return services;
        }

        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            services.Configure<PasskeyOptions>(configuration.GetSection(PasskeyOptions.SectionName));

            var connectionString = BuildPostgresConnectionString(configuration);
            services.AddDbContext<LendingDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IUserCommandRepository, UserRepository>();
            services.AddScoped<IUserQueryRepository, UserRepository>();
            services.AddScoped<IItemCommandRepository, ItemRepository>();
            services.AddScoped<IItemQueryRepository, ItemRepository>();
            services.AddScoped<ILoanCommandRepository, LoanRepository>();
            services.AddScoped<ILoanQueryRepository, LoanRepository>();
            services.AddScoped<IMediaCommandRepository, MediaRepository>();
            services.AddScoped<IQueryConnectionFactory, PostgresQueryConnectionFactory>();
            services.AddScoped<IDatabaseHealthCheck, PostgresHealthCheck>();
            services.AddSingleton<IMessageQueue, InMemoryMessageQueue>();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CommandMessageQueueBehavior<,>));

            services.AddSingleton<IClock, SystemClock>();
            services.AddSingleton<EmailAddressAttribute>();
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

    private static IServiceCollection AddValidatorsFromAssemblies(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var validatorTypes = assemblies
            .SelectMany(assembly => assembly.DefinedTypes)
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Select(type => new
            {
                ImplementationType = type.AsType(),
                ServiceTypes = type
                    .GetInterfaces()
                    .Where(interfaceType =>
                        interfaceType.IsGenericType &&
                        interfaceType.GetGenericTypeDefinition() == typeof(IValidator<>))
                    .ToArray()
            })
            .Where(registration => registration.ServiceTypes.Length > 0);

        foreach (var validatorType in validatorTypes)
        {
            foreach (var serviceType in validatorType.ServiceTypes)
            {
                services.AddScoped(serviceType, validatorType.ImplementationType);
            }
        }

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
