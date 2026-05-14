using LendingSystem.Application.Common;
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
}