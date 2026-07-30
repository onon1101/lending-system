using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Loans;

/// <summary>
/// 借閱時間型別
/// </summary>
/// <remarks>
/// 用於規範借閱時間的型別，
/// 方便 LoansAggregate 能不用將 Dateonly Type 字型轉換成資料庫的時間型別。
/// </remarks>
public sealed record LoanPeriod
{
    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="startDate">開始時間</param>
    /// <param name="endDate">結束時間</param>
    private LoanPeriod(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate; // 開始時間 
        EndDate = endDate; // 結束時間
    }
    
    /// <summary>
    /// 開始借月時間
    /// </summary>
    public DateOnly StartDate { get; }
    
    /// <summary>
    /// 結束借閱時間
    /// </summary>
    public DateOnly EndDate { get; }

    /// <summary>
    /// Static Factory Pattern
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="durationDays"></param>
    /// <returns></returns>
    public static Result<LoanPeriod> Create(DateOnly startDate, int durationDays)
    {
        // 借閱時間小於等於 0 
        if (durationDays <= 0)
        {
            return Result<LoanPeriod>.Failure(LoanDomainError.DurationDaysMustBePositive());
        }

        // 轉換成結束日期
        var endDate = startDate.AddDays(durationDays);

        // 借閱開始時間大於結束時間
        if (startDate >= endDate)
        {
            return Result<LoanPeriod>.Failure(LoanDomainError.StartDateMustBeEarlierThanEndDate());
        }

        return Result<LoanPeriod>.Success(new LoanPeriod(startDate, endDate));
    }
}
