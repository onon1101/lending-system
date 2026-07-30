using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Loans;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans.UpdateLoanRecordTime;

internal sealed class UpdateLoanRecordTimeCommandHandler(
    ILoanCommandRepository loans,
    IItemQueryRepository items,
    IExecutionContextAccessor executionContext,
    IUserAccessChecker userAccessChecker) : IRequestHandler<UpdateLoanRecordTimeCommand, Result<UpdateLoanRecordTimeResult>>
{
    public async Task<Result<UpdateLoanRecordTimeResult>> Handle(UpdateLoanRecordTimeCommand request, CancellationToken cancellationToken)
    {
        long? ownerId = request.UserId;
        if (!string.IsNullOrWhiteSpace(request.OwnerUsername))
        {
            ownerId = await items.GetUserIdByUsernameAsync(request.OwnerUsername, cancellationToken);
        }
        var currentUser = executionContext.Current.User;
        if (ownerId is null || !userAccessChecker.CanAccessUser(currentUser.IsAdmin, currentUser.UserId, ownerId.Value))
        {
            return Result<UpdateLoanRecordTimeResult>.Failure(LoanErrors.ManageOwnItemRecordsOnly());
        }

        var orderId = request.OrderId;
        if (!string.IsNullOrWhiteSpace(request.BorrowingKey) &&
            !PublicResourceKey.TryGetInt("borrowing", request.BorrowingKey, out orderId))
        {
            return Result<UpdateLoanRecordTimeResult>.Failure(LoanErrors.MissingUpdateRecordTimeFields());
        }

        if (ownerId <= 0 ||
            orderId <= 0 ||
            request.StartDate is null && request.EndDate is null)
        {
            return Result<UpdateLoanRecordTimeResult>.Failure(LoanErrors.MissingUpdateRecordTimeFields());
        }

        if (request.StartDate is not null &&
            request.EndDate is not null &&
            request.StartDate >= request.EndDate)
        {
            return Result<UpdateLoanRecordTimeResult>.Failure(LoanDomainError.StartDateMustBeEarlierThanEndDate());
        }

        var loan = await loans.UpdateRecordTimeAsync(
            ownerId.Value,
            orderId,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        return loan.IsSuccess
            ? Result<UpdateLoanRecordTimeResult>.Success(Map(loan.Data!))
            : Result<UpdateLoanRecordTimeResult>.Failure(loan.Error);
    }

    private static UpdateLoanRecordTimeResult Map(UserLoan loan) => new(
        loan.OrderId,
        loan.UserId,
        loan.OrderStartDate,
        loan.OrderEndDate,
        loan.OrderStatus,
        loan.Items.Select(x => new UpdateLoanRecordTimeItemResult(
            x.ObjectDetailId,
            x.ObjectId,
            x.ObjectName,
            x.DetailStatus,
            x.ActualReturnDate)).ToArray());
}
