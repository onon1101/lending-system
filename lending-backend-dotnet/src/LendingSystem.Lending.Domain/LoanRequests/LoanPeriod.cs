using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.LoanRequests;

/// <summary>
/// 借閱者希望的借用日期區間
/// </summary>
public sealed class LoanPeriod 
{
    public LoanPeriod(DateTimeOffset start, DateTimeOffset end)
    {
        Start = start;
        End = end;
    }
    
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    public static Result<LoanPeriod> Create(DateTimeOffset start, DateTimeOffset end)
    {
        var ret = new LoanPeriod(start, end);
        return Result<LoanPeriod>.Success(ret);
    }
}