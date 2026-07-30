using LendingSystem.WebApi.Controllers;
using LendingSystem.WebApi.Models;

namespace LendingSystem.WebApi.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled API exception");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();

            var error = ControllerApiErrors.ServerError();
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Failure(error.Code, error.GetClientMessage(environment.IsDevelopment())));
        }
    }
}
