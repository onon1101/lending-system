using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

public sealed record GetItemLoanHistoryQuery(string OwnerUsername, string ObjectName) : IQuery<IReadOnlyCollection<GetItemLoanHistoryResult>>;
