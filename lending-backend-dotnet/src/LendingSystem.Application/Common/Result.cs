namespace LendingSystem.Application.Common;

public sealed record Error(string Code, string Message)
{
    public static Error None { get; } = new("", "");
}

public sealed record Result<T>
{
    private Result(T? data, bool isSuccess, Error error)
    {
        Data = data;
        IsSuccess = isSuccess;
        Error = error;
    }

    public T? Data { get; }
    public bool IsSuccess { get; }
    public Error Error { get; }

    public static Result<T> Success(T data) => new(data, true, Error.None);

    public static Result<T> Failure(string errorCode, string errorMessage) =>
        new(default, false, new Error(errorCode, errorMessage));
}

public static class ErrorCodes
{
    public const string Validation = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string UnsupportedFileType = "UNSUPPORTED_FILE_TYPE";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string BadGateway = "BAD_GATEWAY";
    public const string ServerError = "SERVER_ERROR";
}
