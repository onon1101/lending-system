using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

internal static class ControllerResultExtensions
{
    public static ActionResult<ApiResponse<T>> ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(ApiResponse<T>.Success(result.Data!));
        }

        return UnwrapErrorMessage<T>(controller, result.Error);
    }

    public static ActionResult<ApiResponse<T>> ToCreatedActionResult<T>(this ControllerBase controller, string location, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Created(location, ApiResponse<T>.Success(result.Data!));
        }

        return UnwrapErrorMessage<T>(controller, result.Error);
    }

    public static ActionResult<ApiResponse<T>> ApiFailure<T>(this ControllerBase controller, Errors errors) =>
        UnwrapErrorMessage<T>(controller, errors);

    public static IActionResult ApiFailureResult(this ControllerBase controller, Errors errors)
    {
        return controller.StatusCode(
            GetStatusCode(errors),
            ApiResponse<object>.Failure(errors.Code, errors.ErrorMessage));
    }

    /// <summary>
    private static ActionResult<ApiResponse<T>> UnwrapErrorMessage<T>(ControllerBase controller, Errors errors)
    {
        return controller.StatusCode(
            GetStatusCode(errors), 
            ApiResponse<T>.Failure(errors.Code, errors.ErrorMessage));
    }

    private static int GetStatusCode(Errors errors) =>
        errors.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.BadGateway => StatusCodes.Status502BadGateway,
            ErrorType.ServiceUnavailable => StatusCodes.Status503ServiceUnavailable,
            ErrorType.ServerError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
}
