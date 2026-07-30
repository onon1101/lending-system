using LendingSystem.WebApi.Middleware;
using LendingSystem.WebApi.Modules.Definitions;

namespace LendingSystem.WebApi.Modules.ExceptionHandling;

public sealed class ExceptionHandlingModule : ModuleInstaller
{
    public override IApplicationBuilder UseApplication(WebApplication app)
    {
        app.UseMiddleware<ApiExceptionMiddleware>();
        return app;
    }
}
