using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Lending.Application.Loans.GetLoanRequestByUser;

public sealed record GetLoanRequestByUserQuery : IQuery<IReadOnlyCollection<GetLoanRequestByUserResult>>;
