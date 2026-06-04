using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans.DeleteLoanRecord;

internal sealed class DeleteLoanRecordCommandHandler(
    ILoanCommandRepository loans,
    IItemQueryRepository items,
    IExecutionContextAccessor executionContext,
    IUserAccessChecker userAccessChecker) : IRequestHandler<DeleteLoanRecordCommand, Result<DeleteLoanRecordResult>>
{
    public async Task<Result<DeleteLoanRecordResult>> Handle(DeleteLoanRecordCommand request, CancellationToken cancellationToken)
    {
        var ownerId = await items.GetUserIdByUsernameAsync(request.OwnerUsername, cancellationToken);
        var currentUser = executionContext.Current.User;
        if (ownerId is null || !userAccessChecker.CanAccessUser(currentUser.IsAdmin, currentUser.UserId, ownerId.Value))
        {
            return Result<DeleteLoanRecordResult>.Failure(LoanErrors.ManageOwnItemRecordsOnly());
        }

        if (!PublicResourceKey.TryGetInt("borrowing", request.BorrowingKey, out var orderId))
        {
            return Result<DeleteLoanRecordResult>.Failure(LoanErrors.MissingDeleteRecordFields());
        }

        if (ownerId <= 0 || orderId <= 0)
        {
            return Result<DeleteLoanRecordResult>.Failure(LoanErrors.MissingDeleteRecordFields());
        }

        var deleted = await loans.DeleteRecordAsync(ownerId.Value, orderId, cancellationToken);
        return deleted.IsSuccess
            ? Result<DeleteLoanRecordResult>.Success(new DeleteLoanRecordResult(true, "Delete borrowing record is successful."))
            : Result<DeleteLoanRecordResult>.Failure(deleted.Error);
    }
}
