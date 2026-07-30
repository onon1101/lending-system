using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Auth.Application.Auth;

public static class AuthErrors
{
    public static Errors UserIsExists() =>
        new("Auth.AlreadyExists", "User already exists");

    public static Errors InvalidEmail() =>
        new("Auth.InvalidEmail", "Invalid email");

    public static Errors InvalidCredentials() =>
        new("Auth.InvalidCredentials", "Invalid account or password", ErrorType.Unauthorized);

    public static Errors InvalidRequestBody() =>
        new("Auth.InvalidRequestBody", "Invalid request body");

    public static Errors UserNotFound() =>
        new("Auth.UserNotFound", "User not found", ErrorType.NotFound);

    public static Errors DeleteUserFailed() =>
        new("Auth.DeleteUserFailed", "Delete user failed", ErrorType.ServerError);

    public static Errors GoogleTokenRequired() =>
        new("Auth.GoogleTokenRequired", "Google token is required");

    public static Errors GoogleClientIdNotConfigured() =>
        new("Auth.GoogleClientIdNotConfigured", "Server configuration error", ErrorType.ServerError);

    public static Errors GoogleLoginFailed() =>
        new("Auth.GoogleLoginFailed", "Google login failed", ErrorType.Unauthorized);

    public static Errors GoogleAccountNotVerified() =>
        new("Auth.GoogleAccountNotVerified", "Google account is not verified", ErrorType.Unauthorized);

    public static Errors GoogleAccountLinkFailed() =>
        new("Auth.GoogleAccountLinkFailed", "Google account link failed", ErrorType.Unauthorized);
}
