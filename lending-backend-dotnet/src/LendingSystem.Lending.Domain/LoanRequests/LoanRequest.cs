using LendingSystem.SharedKernel.Domain.Abstractions;

namespace LendingSystem.Lending.Domain.LoanRequests;

/// <summary>
/// 借閱申請
/// </summary>
public sealed class LoanRequest : IAggregateRoot
{
    private LoanRequest()
    {

    }

    /// <summary>
    /// 訂單編號
    /// </summary>
    public long RequestId { get; }
    
    /// <summary>
    /// 被借閱者 ID
    /// </summary>
    public long BorrowerId { get; }
    
    /// <summary>
    /// 物品 ID
    /// </summary>
    public long ItemId { get; }
    
    /// <summary>
    /// 借閱者希望借閱的日期區間
    /// </summary>
    public LoanPeriod RequestPeriod { get; }
    
    /// <summary>
    /// 審核狀態
    /// </summary>
    public LoanRequestStatus Status { get; }
    
    /// <summary>
    /// 請求建立日期
    /// </summary>
    public DateTimeOffset CreatedAt { get; }
    
    /// <summary>
    /// 核准日期
    /// </summary>
    public DateTimeOffset? DecidedAt { get; }
    
    /// <summary>
    /// 備註
    /// </summary>
    public string? DecisionReson { get; }
}
