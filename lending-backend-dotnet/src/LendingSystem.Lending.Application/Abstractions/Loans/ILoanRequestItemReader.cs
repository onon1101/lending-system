using LendingSystem.Lending.Application.Commons;

namespace LendingSystem.Lending.Application.Abstractions;

public interface ILoanRequestItemReader
{
    Task<LoanRequestItem?> GetAsync(
        string itemOwnerUsername,
        string itemName,
        CancellationToken cancellationToken);
}