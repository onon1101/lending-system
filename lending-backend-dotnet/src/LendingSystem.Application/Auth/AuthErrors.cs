using LendingSystem.Domain.Commons;

namespace LendingSystem.Application.Auth;

public static class AuthErrors
{
    public static ApplicationErrors InvalidEmail() =>
        new("AUTH_INVALID_EMAIL", "錯誤的 Email 格式", "Invalid email", ErrorType.Validation);

    public static ApplicationErrors InvalidCredentials() =>
        new("AUTH_INVALID_CREDENTIALS", "帳號或密碼錯誤", "Invalid account or password", ErrorType.Unauthorized);

    public static ApplicationErrors InvalidRequestBody() =>
        new("AUTH_INVALID_REQUEST_BODY", "Invalid request body", "Invalid request body", ErrorType.Validation);

    public static ApplicationErrors UserNotFound() =>
        new("AUTH_USER_NOT_FOUND", "User not found", "User not found", ErrorType.NotFound);

    public static ApplicationErrors DeleteUserFailed() =>
        new("AUTH_DELETE_USER_FAILED", "Delete user is not successful.", "Delete user failed", ErrorType.ServerError);

    public static ApplicationErrors GoogleTokenRequired() =>
        new("AUTH_GOOGLE_TOKEN_REQUIRED", "Google token is required", "Google token is required", ErrorType.Validation);

    public static ApplicationErrors GoogleClientIdNotConfigured() =>
        new("AUTH_GOOGLE_CLIENT_ID_NOT_CONFIGURED", "Google ClientId is not configured", "Server configuration error", ErrorType.ServerError);

    public static ApplicationErrors GoogleLoginFailed() =>
        new("AUTH_GOOGLE_LOGIN_FAILED", "Google 登入驗證失敗", "Google login failed", ErrorType.Unauthorized);

    public static ApplicationErrors GoogleAccountNotVerified() =>
        new("AUTH_GOOGLE_ACCOUNT_NOT_VERIFIED", "Google 帳號資料未驗證", "Google account is not verified", ErrorType.Unauthorized);

    public static ApplicationErrors GoogleAccountLinkFailed() =>
        new("AUTH_GOOGLE_ACCOUNT_LINK_FAILED", "Google 帳號連結失敗", "Google account link failed", ErrorType.Unauthorized);
}
