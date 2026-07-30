using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Loans;

public sealed class Loan : Entity
{
    private Loan(
        long orderId,
        long itemId,
        DateOnly startDate,
        DateOnly endDate,
        DateOnly? actualReturnDate,
        string status)
    {
        OrderId = orderId;
        ItemId = itemId;
        StartDate = startDate;
        EndDate = endDate;
        ActualReturnDate = actualReturnDate;
        Status = status;
    }

    public long OrderId { get; }
    public long ItemId { get; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public DateOnly? ActualReturnDate { get; private set; }
    public string Status { get; private set; }

    public static Loan Create(long itemId, DateOnly startDate, DateOnly endDate) =>
        new(0, itemId, startDate, endDate, null, LoanStatuses.OnLoan);

    public static Loan CreateRequest(long itemId, DateOnly startDate, DateOnly endDate) =>
        new(0, itemId, startDate, endDate, null, LoanStatuses.Requested);

    public static Loan Rehydrate(
        long orderId,
        long itemId,
        DateOnly startDate,
        DateOnly endDate,
        DateOnly? actualReturnDate,
        string status) =>
        new(orderId, itemId, startDate, endDate, actualReturnDate, status);

    public void Return(DateOnly actualReturnDate)
    {
        Status = LoanStatuses.Returned;
        ActualReturnDate = actualReturnDate;
    }

    public void ChangePeriod(DateOnly startDate, DateOnly endDate)
    {
        if (startDate >= endDate)
        {
            throw new InvalidOperationException("Loan start date must be earlier than end date.");
        }

        StartDate = startDate;
        EndDate = endDate;
    }

    public static Loan CreateRequest(long itemId, LoanPeriod period)
    {
        return new Loan(
            0,
            itemId,
            period.StartDate,
            period.EndDate,
            null,
            LoanStatuses.Requested);
    }

    /// <summary>
    /// 同意借閱
    /// </summary>
    /// <returns></returns>
    public Result<Loan> Approve()
    {
        if (Status != LoanStatuses.Requested)
        {
            return Result<Loan>.Failure(LoanDomainError.OnlyRequestedLoanCanBeApproved());
        }

        Status = LoanStatuses.Approved;
        return Result<Loan>.Success(this);
    }

    /// <summary>
    /// 拒絕借閱
    /// </summary>
    /// <returns></returns>
    public Result<Loan> Reject()
    {
        if (Status != LoanStatuses.Requested)
        {
            return Result<Loan>.Failure(LoanDomainError.OnlyRequestedLoanCanBeRejected());
        }

        Status = LoanStatuses.Rejected;
        return Result<Loan>.Success(this);
    }
}
