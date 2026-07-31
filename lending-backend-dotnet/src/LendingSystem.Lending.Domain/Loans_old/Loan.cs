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
<<<<<<<< HEAD:lending-backend-dotnet/src/LendingSystem.Lending.Domain/Loans_old/Loan.cs
    /// <returns></returns>
    public Result<Loan> Approve()
    {
        if (Status != LoanStatuses.Requested)
        {
            return Result<Loan>.Failure(LoanErrors.OnlyRequestedLoanCanBeApproved());
        }

        Status = LoanStatuses.Approved;
        return Result<Loan>.Success(this);
    }

    /// <summary>
    /// 拒絕借閱
    /// </summary>
    /// <returns></returns>
    public Result<Loan> Reject()
    {
        if (Status != LoanStatuses.Requested)
        {
            return Result<Loan>.Failure(LoanErrors.OnlyRequestedLoanCanBeRejected());
        }

        Status = LoanStatuses.Rejected;
        return Result<Loan>.Success(this);
    }
}
========
    public long LoanId { get; }
}
>>>>>>>> fdd49cfca8d4aa9e598fa63008c0a0cf8ccf5019:lending-backend-dotnet/src/LendingSystem.Lending.Domain/Loans/Loan.cs
