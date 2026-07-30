namespace LendingSystem.Lending.Domain.Loans;

public sealed record LoanRequestRecord(
    long OrderId,
    string ItemName,
    string BorrowerName,
    string BorrowerUsername,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status);