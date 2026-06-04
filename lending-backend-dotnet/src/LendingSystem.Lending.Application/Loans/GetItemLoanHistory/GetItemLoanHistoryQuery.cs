using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Loans.GetItemLoanHistory;

public sealed record GetItemLoanHistoryQuery(string OwnerUsername, string ObjectName) : IQuery<IReadOnlyCollection<GetItemLoanHistoryResult>>;
