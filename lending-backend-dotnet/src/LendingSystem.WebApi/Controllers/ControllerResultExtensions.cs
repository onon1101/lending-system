using LendingSystem.Application.Common;
using LendingSystem.WebApi.Models;
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

        return controller.StatusCode(GetStatusCode(result.Error.Code), ApiResponse<T>.Failure(result.Error.Code, result.Error.Message));
    }

    public static ActionResult<ApiResponse<T>> ToCreatedActionResult<T>(this ControllerBase controller, string location, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Created(location, ApiResponse<T>.Success(result.Data!));
        }

        return controller.StatusCode(GetStatusCode(result.Error.Code), ApiResponse<T>.Failure(result.Error.Code, result.Error.Message));
    }

    public static ActionResult<ApiResponse<T>> ApiFailure<T>(this ControllerBase controller, string errorCode, string errorMessage) =>
        controller.StatusCode(GetStatusCode(errorCode), ApiResponse<T>.Failure(errorCode, errorMessage));

    private static int GetStatusCode(string errorCode) =>
        errorCode switch
        {
            ErrorCodes.NotFound => StatusCodes.Status404NotFound,
            ErrorCodes.Conflict => StatusCodes.Status409Conflict,
            ErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCodes.BadGateway => StatusCodes.Status502BadGateway,
            ErrorCodes.ServerError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
}
