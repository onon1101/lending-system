using LendingSystem.SharedKernel.Domain.Abstractions;
using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Loans;

public sealed class Loan : IAggregateRoot
{
    private Loan() { }
    public static Result<Loan> Create()
    {

    }
}
