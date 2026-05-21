using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

public sealed record GetUserActiveLoansQuery(int UserId) : IQuery<IReadOnlyCollection<GetUserActiveLoansResult>>;
