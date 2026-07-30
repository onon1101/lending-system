using LendingSystem.WebApi.Controllers;

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

            var error = ControllerApiErrors.ServerError();
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Failure(error.Code, error.ErrorMessage));
        }
    }
}
