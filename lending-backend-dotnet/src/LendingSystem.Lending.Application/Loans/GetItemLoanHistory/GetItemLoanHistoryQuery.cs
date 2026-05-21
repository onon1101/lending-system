using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

public sealed record GetItemLoanHistoryQuery(int ItemId) : IQuery<IReadOnlyCollection<GetItemLoanHistoryResult>>;
