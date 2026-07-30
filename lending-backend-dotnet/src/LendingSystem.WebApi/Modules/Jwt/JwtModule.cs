using System.Security.Claims;
using System.Text;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.Auth.Infrastructure.Auth;
using LendingSystem.SharedKernel.Domain.Common;
using LendingSystem.WebApi.Controllers;
using LendingSystem.WebApi.Modules.Definitions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace LendingSystem.WebApi.Modules.Jwt;

public sealed class JwtModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var secretKey = configuration["SECRET_KEY"]
            ?? configuration["Jwt:SecretKey"]
            ?? "development-secret-key-change-before-production";

        services.AddSingleton<ITokenService, JwtTokenService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(ToFailureResponse(
                            ControllerApiErrors.Unauthorized(),
                            environment.IsDevelopment()));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(ToFailureResponse(
                            ControllerApiErrors.Forbidden(),
                            environment.IsDevelopment()));
                    }
                };
            });

        return services;
    }

    public override IApplicationBuilder UseApplication(WebApplication app)
    {
        app.UseAuthentication();
        return app;
    }

    private static ApiResponse<object> ToFailureResponse(
        Errors error,
        bool isDevelopment) =>
        ApiResponse<object>.Failure(
            error.Code,
            error.GetClientMessage(isDevelopment));
}
