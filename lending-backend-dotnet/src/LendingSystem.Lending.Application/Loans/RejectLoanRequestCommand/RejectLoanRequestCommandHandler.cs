using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Loans;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans.RejectLoanRequestCommand;

internal sealed class RejectLoanRequestCommandHandler(
    ILoanRequestDecisionReader loansRequest,
    ILoanCommandRepository loans,
    IExecutionContextAccessor executionContext)
    : IRequestHandler<RejectLoanRequestCommand, Result<RejectLoanRequestResult>>
{
    public async Task<Result<RejectLoanRequestResult>> Handle(
        RejectLoanRequestCommand request,
        CancellationToken cancellationToken)
    {
        var currentUserId = executionContext.Current.User.UserId;
        if (currentUserId <= 0 ||
            !PublicResourceKey.TryGetInt("borrowing", request.BorrowingKey, out var orderId) ||
            orderId <= 0)
        {
            return Result<RejectLoanRequestResult>.Failure(LoanErrors.MissingLoanRequestDecisionFields());
        }

        var context = await loansRequest.GetAsync(currentUserId, orderId, cancellationToken);
        if (context is null)
        {
            return Result<RejectLoanRequestResult>.Failure(LoanErrors.LoanRequestNotFound());
        }

        var rejected = context.Loan.Reject();
        if (!rejected.IsSuccess)
        {
            return Result<RejectLoanRequestResult>.Failure(rejected.Error);
        }

        var saved = await loans.SaveDecisionAsync(rejected.Data!, cancellationToken);
        return saved.IsSuccess
            ? Result<RejectLoanRequestResult>.Success(Map(saved.Data!))
            : Result<RejectLoanRequestResult>.Failure(saved.Error);
    }

    private static RejectLoanRequestResult Map(UserLoan loan) => new(
        loan.OrderId,
        loan.OrderStatus);
}
