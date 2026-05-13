namespace LendingSystem.Domain.Loans;

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
    DateTimeOffset? ActualReturnTime);

public sealed record UserLoan(
    int OrderId,
    int UserId,
    DateTimeOffset OrderStartTime,
    DateTimeOffset OrderEndTime,
    string OrderStatus,
    IReadOnlyCollection<LoanItemDetail> Items);

public sealed record LoanRecord(
    int? OrderId,
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    string? Name,
    string? Status);
