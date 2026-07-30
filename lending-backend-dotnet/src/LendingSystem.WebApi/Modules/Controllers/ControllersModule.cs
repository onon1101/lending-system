using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Domain.Common;
using LendingSystem.WebApi.Controllers;
using LendingSystem.WebApi.Models;
using LendingSystem.WebApi.Modules.Definitions;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Modules.Controllers;

public sealed class ControllersModule : ModuleInstaller
{
    public override IServiceCollection InstallServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition =
                    JsonIgnoreCondition.Never;
            });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var message = string.Join("; ", context.ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "Invalid request body"
                        : error.ErrorMessage));

                return new BadRequestObjectResult(ToFailureResponse(
                    ControllerApiErrors.InvalidRequestBody(message),
                    environment.IsDevelopment()));
            };
        });

        return services;
    }

    public override IApplicationBuilder UseApplication(WebApplication app)
    {
        app.MapControllers();
        return app;
    }

    private static ApiResponse<object> ToFailureResponse(
        Errors error,
        bool isDevelopment) =>
        ApiResponse<object>.Failure(
            error.Code,
            error.GetClientMessage(isDevelopment));
}
