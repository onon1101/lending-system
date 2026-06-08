namespace LendingSystem.Lending.Domain.Aggregates.Loans;

public sealed record UserLoan(
    long OrderId,
    long UserId,
    DateOnly OrderStartDate,
    DateOnly OrderEndDate,
    string OrderStatus,
    IReadOnlyCollection<LoanItemDetail> Items);

