namespace LendingSystem.Lending.Domain.Aggregates.Loans;

public sealed record LoanRecord(
    long? OrderId,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Name,
    string? Status);

