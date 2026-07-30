namespace LendingSystem.Lending.Domain.LoanRequests;

public sealed class LoanRequestStatus : Enumeration<int, string>
{
    /// <summary>
    /// 審閱中
    /// </summary>
    public static readonly LoanRequestStatus Pending =
        new(0, nameof(Pending));

    /// <summary>
    /// 同意
    /// </summary>
    public static readonly LoanRequestStatus Approved =
        new(1, nameof(Approved));

    /// <summary>
    /// 不同意
    /// </summary>
    public static readonly LoanRequestStatus Rejected =
        new(2, nameof(Rejected));

    /// <summary>
    /// 取消
    /// </summary>
    public static readonly LoanRequestStatus Cancelled =
        new(3, nameof(Cancelled));

    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public LoanRequestStatus(int key, string value) : base(key, value)
    {
    }
}
