using LendingSystem.Domain.Commons;

namespace LendingSystem.Application.Common;

public sealed record Result<T>
{
    private Result(T? data, bool isSuccess, Errors error)
    {
        Data = data;
        IsSuccess = isSuccess;
        Error = error;
    }

    public T? Data { get; }
    public bool IsSuccess { get; }
    public Errors Error { get; }

    public static Result<T> Success(T data) => new(data, true, Errors.None);

    public static Result<T> Failure(Errors error) =>
        new(default, false, error);
}
