namespace LendingSystem.Lending.Domain.Aggregates.Loans;

public sealed record LoanItemDetail(
    long ObjectDetailId,
    long ObjectId,
    string ObjectName,
    string DetailStatus,
    DateOnly? ActualReturnDate);

