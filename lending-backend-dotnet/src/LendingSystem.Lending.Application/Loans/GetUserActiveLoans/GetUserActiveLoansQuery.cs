using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Loans.GetUserActiveLoans;

public sealed record GetUserActiveLoansQuery(string Username) : IQuery<IReadOnlyCollection<GetUserActiveLoansResult>>;
