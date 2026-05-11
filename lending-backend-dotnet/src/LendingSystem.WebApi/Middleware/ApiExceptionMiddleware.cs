using LendingSystem.Application.Common;
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
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Failure(ErrorCodes.ServerError, "Failed to process request due to server error"));
        }
    }
}
