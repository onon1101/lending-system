namespace LendingSystem.Domain.Loans;

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
    DateTimeOffset? StartTime,
    DateTimeOffset? EndTime,
    string? Name,
    string? Status);
