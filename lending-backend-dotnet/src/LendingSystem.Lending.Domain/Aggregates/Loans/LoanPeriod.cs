using LendingSystem.SharedKernel.Domain.Abstractions;
using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Aggregates.Loans;

public sealed record LoanPeriod
{
    private LoanPeriod(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
    
    public DateOnly StartDate { get; }
    public DateOnly EndDate { get; }

    public static LoanPeriod Create(DateOnly startDate, int durationDays)
    {
        if (durationDays <= 0)
        {
            throw new BusinessRuleValidationException(
                new DurationDaysMustBePositiveRule(durationDays));
        }

        var endDate = startDate.AddDays(durationDays);

        if (startDate >= endDate)
        {
            throw new BusinessRuleValidationException(
                new LoanStartDateMustBeEarlierThanEndDateRule(startDate, endDate));
        }

        return new LoanPeriod(startDate, endDate);
    }
}

public sealed class DurationDaysMustBePositiveRule(int durationDays) : IBusinessRule
{
    public bool IsBroken() => durationDays <= 0;

    public string Message => "Loan duration days must be positive.";
}

public sealed class LoanStartDateMustBeEarlierThanEndDateRule(DateOnly startDate, DateOnly endDate) : IBusinessRule
{
    public bool IsBroken() => startDate >= endDate;

    public string Message => "Loan start date must be earlier than end date.";
}
