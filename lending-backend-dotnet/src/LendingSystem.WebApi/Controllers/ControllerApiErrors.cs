using LendingSystem.Domain.Commons;

namespace LendingSystem.WebApi.Controllers;

public static class ControllerApiErrors
{
    /// <summary>
    /// AccessToken 或者 RefreshToken 為無效
    /// </summary>
    /// <returns></returns>
    public static ControllerErrors TokenInvalid() => 
        new ("Invalid_Token", 
            "Invalid Token", 
            "Invalid Token",
            ErrorType.Unauthorized);

    /// <summary>
    /// Client's 請求缺少檔案
    /// </summary>
    /// <returns></returns>
    public static ControllerErrors MissingFiles() =>
        new (
            "Missing_Files", 
            "Missing Files",
            "Missing Files",
            ErrorType.Validation);

    /// <summary>
    /// Client's 請求缺少特定欄位
    /// </summary>
    /// <returns></returns>
    public static ControllerErrors MissingField(string missingField) =>
        new ("Missing_Field",
                    $"Missing Field: {missingField}",
                    "Missing Field",
                    ErrorType.Validation);

    /// <summary>
    /// Client's 請求中，某欄位要求要為整數值
    /// </summary>
    /// <param name="Field"></param>
    /// <returns></returns>
    public static ControllerErrors MustBeInteger(string missingField) =>
        new("Must_Be_Integer",
            $"{missingField} must be integer",
            "Some field required type integer",
            ErrorType.Validation);

    public static ControllerErrors AccessOwnBorrowingsOnly() =>
        new(
            "ACCESS_OWN_BORROWINGS_ONLY",
            "You can only access your own borrowings",
            "You can only access your own borrowings",
            ErrorType.Unauthorized);

    public static ControllerErrors CreateBorrowingsForSelfOnly() =>
        new(
            "CREATE_BORROWINGS_FOR_SELF_ONLY",
            "You can only create borrowings for yourself",
            "You can only create borrowings for yourself",
            ErrorType.Unauthorized);

    public static ControllerErrors ManageOwnItemRecordsOnly() =>
        new(
            "MANAGE_OWN_ITEM_RECORDS_ONLY",
            "You can only manage your own item records",
            "You can only manage your own item records",
            ErrorType.Unauthorized);

    public static ControllerErrors InvalidRequestBody(string details) =>
        new(
            "INVALID_REQUEST_BODY",
            string.IsNullOrWhiteSpace(details) ? "Invalid request body" : details,
            "Invalid request body",
            ErrorType.Validation);

    public static ControllerErrors Unauthorized() =>
        new("UNAUTHORIZED", "Unauthorized", "Unauthorized", ErrorType.Unauthorized);

    public static ControllerErrors Forbidden() =>
        new("FORBIDDEN", "Forbidden", "Forbidden", ErrorType.Forbidden);

    public static ControllerErrors ServiceUnavailable() =>
        new("SERVICE_UNAVAILABLE", "Service is unavailable", "Service is unavailable", ErrorType.ServiceUnavailable);

    public static ControllerErrors InvalidAbsoluteUrl() =>
        new(
            "INVALID_ABSOLUTE_URL",
            "url query parameter must be a valid absolute URL",
            "url query parameter must be a valid absolute URL",
            ErrorType.Validation);

    public static ControllerErrors BadGateway(string message) =>
        new(
            "BAD_GATEWAY",
            string.IsNullOrWhiteSpace(message) ? "Bad gateway" : message,
            "Bad gateway",
            ErrorType.BadGateway);

    public static ControllerErrors ServerError() =>
        new(
            "SERVER_ERROR",
            "Failed to process request due to server error",
            "Failed to process request due to server error",
            ErrorType.ServerError);
}
