using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.WebApi.Controllers;

public static class ControllerApiErrors
{
    /// <summary>
    /// AccessToken 或者 RefreshToken 為無效
    /// </summary>
    /// <returns></returns>
    public static Errors TokenInvalid() => 
        new("Api.InvalidToken", "Invalid Token", ErrorType.Unauthorized);

    /// <summary>
    /// Client's 請求缺少檔案
    /// </summary>
    /// <returns></returns>
    public static Errors MissingFiles() =>
        new("Api.MissingFiles", "Missing Files", ErrorType.Validation);

    /// <summary>
    /// Client's 請求缺少特定欄位
    /// </summary>
    /// <returns></returns>
    public static Errors MissingField(string missingField) =>
        new("Api.MissingField", "Missing Field", ErrorType.Validation);

    /// <summary>
    /// Client's 請求中，某欄位要求要為整數值
    /// </summary>
    /// <param name="missingField"></param>
    /// <returns></returns>
    public static Errors MustBeInteger(string missingField) =>
        new("Api.MustBeInteger", "Some field required type integer", ErrorType.Validation);

    public static Errors AccessOwnBorrowingsOnly() =>
        new("Api.AccessOwnBorrowingsOnly", "You can only access your own borrowings", ErrorType.Unauthorized);

    public static Errors CreateBorrowingsForSelfOnly() =>
        new("Api.CreateBorrowingsForSelfOnly", "You can only create borrowings for yourself", ErrorType.Unauthorized);

    public static Errors ManageOwnItemRecordsOnly() =>
        new("Api.ManageOwnItemRecordsOnly", "You can only manage your own item records", ErrorType.Unauthorized);

    public static Errors InvalidRequestBody(string details) =>
        new("Api.InvalidRequestBody", "Invalid request body", ErrorType.Validation);

    public static Errors Unauthorized() =>
        new("Api.Unauthorized", "Unauthorized", ErrorType.Unauthorized);

    public static Errors Forbidden() =>
        new("Api.Forbidden", "Forbidden", ErrorType.Forbidden);

    public static Errors ServiceUnavailable() =>
        new("Api.ServiceUnavailable", "Service is unavailable", ErrorType.ServiceUnavailable);

    public static Errors InvalidAbsoluteUrl() =>
        new("Api.InvalidAbsoluteUrl", "url query parameter must be a valid absolute URL", ErrorType.Validation);

    public static Errors BadGateway(string message) =>
        new("Api.BadGateway", "Bad gateway", ErrorType.BadGateway);

    public static Errors ServerError() =>
        new("Api.ServerError", "Failed to process request due to server error", ErrorType.ServerError);
}
