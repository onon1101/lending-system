using LendingSystem.Lending.Domain.Aggregates.Loans;

namespace LendingSystem.Lending.Application.Commons;

public sealed record LoanRequestDecisionContext(
    Loan Loan,
    long OwnerId,
    long ItemId,
    string ItemStatus);