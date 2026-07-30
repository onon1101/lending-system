using System.Diagnostics;
using LendingSystem.SharedKernel.Domain.Abstractions;

namespace LendingSystem.Lending.Domain.Loans;

public sealed class Loan : IAggregateRoot
{
    private Loan()
    {
        
    }
    
    /// <summary>
    /// 借閱 ID
    /// </summary>
    public long LoanId { get; }
}