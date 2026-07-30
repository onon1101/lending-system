namespace LendingSystem.Lending.Domain.Loans;

/// <summary>
/// 借閱狀態
/// </summary>
public static class LoanStatuses
{
    /// <summary>
    /// 已經送出請求給
    /// </summary>
    public const string Requested = "Requested";
    
    /// <summary>
    /// 已同意
    /// </summary>
    public const string Approved = "Approved";
    
    /// <summary>
    /// 已拒絕
    /// </summary>
    public const string Rejected = "Rejected";

    /// <summary>
    /// 已經由借閱者拿取，正在使用
    /// </summary>
    public const string OnLoan = "On Loan";

    /// <summary>
    /// 已歸還
    /// </summary>
    public const string Returned = "Returned";
}