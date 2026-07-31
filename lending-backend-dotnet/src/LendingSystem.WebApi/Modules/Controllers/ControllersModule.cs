using System.Text.Json.Serialization;
using LendingSystem.Auth.WebApi;
using LendingSystem.Lending.WebApi;
using LendingSystem.SharedKernel.Domain.Common;
using LendingSystem.WebApi.Controllers;
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
            .AddApplicationPart(typeof(AuthWebApiAssembly).Assembly)
            .AddApplicationPart(typeof(LendingWebApiAssembly).Assembly)
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
                    ControllerApiErrors.InvalidRequestBody(message)));
            };
        });

        return services;
    }

    public override IApplicationBuilder UseApplication(WebApplication app)
    {
        app.MapControllers();
        return app;
    }

    private static ApiResponse<object> ToFailureResponse(Errors error) =>
        ApiResponse<object>.Failure(
            error.Code,
            error.ErrorMessage);
}
