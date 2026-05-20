namespace LendingSystem.Lending.Domain.Loans;

public static class LoanStatuses
{
    public const string OnLoan = "On Loan";
    public const string Returned = "Returned";
}

public sealed record LoanItemDetail(
    int ObjectDetailId,
    int ObjectId,
    string ObjectName,
    string DetailStatus,
    DateOnly? ActualReturnDate);

public sealed record UserLoan(
    int OrderId,
    int UserId,
    DateOnly OrderStartDate,
    DateOnly OrderEndDate,
    string OrderStatus,
    IReadOnlyCollection<LoanItemDetail> Items);

public sealed record LoanRecord(
    int? OrderId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Name,
    string? Status);
