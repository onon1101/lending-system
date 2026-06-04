namespace LendingSystem.SharedKernel.Domain.Common;

/// <summary>
/// 回傳資料結果型別
/// </summary>
/// <remarks>
/// 用於 Application 和 Domain Layer 作為資料回傳的 Wrapper，
/// 由於傳統的 Exception 的太慢，並且是獨立的 Exception Flow，
/// 導致語意不清楚。
///
/// 所以使用 Result Pattern 解決上述問題。
/// </remarks>
/// <typeparam name="T"></typeparam>
public sealed record Result<T>
{
    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="data"></param>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    private Result(T? data, bool isSuccess, Errors error)
    {
        Data = data;
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Wrapper 裡面的資料
    /// </summary>
    public T? Data { get; }
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; }
    
    /// <summary>
    /// 是否錯誤
    /// </summary>
    public Errors Error { get; }

    /// <summary>
    /// Static Factory for create itself in successful circumstance.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Result<T> Success(T data) => new(data, true, Errors.None);

    /// <summary>
    /// Static Factory for create itself in failure circumstance.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<T> Failure(Errors error) =>
        new(default, false, error);
}
