using LendingSystem.Lending.Application.Commons;

namespace LendingSystem.Lending.Application.Abstractions;

public interface ILoanRequestDecisionReader
{
    Task<LoanRequestDecisionContext?> GetAsync(
        long ownerId,
        long orderId,
        CancellationToken cancellationToken = default);
}