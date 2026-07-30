using FluentValidation;
using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Loans;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans.ApproveLoanRequestCommand;

internal sealed class ApproveLoanRequestCommandHandler(
    ILoanRequestDecisionReader loanRequests,
    ILoanCommandRepository loans,
    IExecutionContextAccessor executionContext,
    IValidator<ApproveLoanRequestCommand> validator)
    : IRequestHandler<ApproveLoanRequestCommand, Result<ApproveLoanRequestResult>>
{
    public async Task<Result<ApproveLoanRequestResult>> Handle(
        ApproveLoanRequestCommand request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        var currentUserId = executionContext.Current.User.UserId;
        if (!validation.IsValid || currentUserId <= 0)
        {
            return Result<ApproveLoanRequestResult>.Failure(LoanErrors.MissingLoanRequestDecisionFields());
        }

        PublicResourceKey.TryGetInt("borrowing", request.BorrowingKey, out var orderId);

        var context = await loanRequests.GetAsync(
            currentUserId,
            orderId, 
            cancellationToken);
        if (context is null)
        {
            return Result<ApproveLoanRequestResult>.Failure(
                LoanErrors.LoanRequestNotFound());
        }

        var approved = context.Loan.Approve();
        if (!approved.IsSuccess)
        {
            return Result<ApproveLoanRequestResult>.Failure(approved.Error);
        }

        var saved = await loans.SaveDecisionAsync(approved.Data!, cancellationToken);
        return saved.IsSuccess
            ? Result<ApproveLoanRequestResult>.Success(Map(saved.Data!))
            : Result<ApproveLoanRequestResult>.Failure(saved.Error);
    }

    private static ApproveLoanRequestResult Map(UserLoan loan) => new(
        loan.OrderId,
        loan.OrderStatus);
}
