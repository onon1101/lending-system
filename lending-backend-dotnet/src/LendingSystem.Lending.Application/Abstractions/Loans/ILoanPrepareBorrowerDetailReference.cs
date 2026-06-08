using LendingSystem.Lending.Application.Commons;

namespace LendingSystem.Lending.Application.Abstractions.Loans;

public interface ILoanPrepareBorrowerDetailReference
{
    Task<LoanBorrowerDetail> GetAsync(
        long borrowerId,
        string borrowerName,
        long ownerId,
        DateOnly today,
        CancellationToken cancellationToken);
}