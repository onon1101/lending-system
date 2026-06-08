namespace LendingSystem.Lending.Application.Commons;

public sealed record LoanRequestItem(
    long ItemId,
    string ItemName,
    string CurrentStatus,
    long OwnerId,
    string OwnerName);