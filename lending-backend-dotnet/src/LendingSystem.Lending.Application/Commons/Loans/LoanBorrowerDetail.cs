namespace LendingSystem.Lending.Application.Commons;

public sealed record LoanBorrowerDetail(
    long BorrowerDetailId,
    long BorrowerUserId,
    string BorrowerName,
    long OwnerId,
    bool IsNew);