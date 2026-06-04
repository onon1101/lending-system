using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Aggregates.Loans;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans.CreateLoanRecord;

internal sealed class CreateLoanRecordCommandHandler(
    ILoanCommandRepository loans,
    IItemQueryRepository items,
    IExecutionContextAccessor executionContext,
    IUserAccessChecker userAccessChecker) : IRequestHandler<CreateLoanRecordCommand, Result<CreateLoanRecordResult>>
{
    public async Task<Result<CreateLoanRecordResult>> Handle(CreateLoanRecordCommand request, CancellationToken cancellationToken)
    {
        var ownerUsername = request.OwnerUsername ?? "";
        var item = !string.IsNullOrWhiteSpace(ownerUsername) && !string.IsNullOrWhiteSpace(request.ObjectName)
            ? await items.GetByNameAsync(ownerUsername, request.ObjectName, cancellationToken)
            : null;
        var ownerId = item?.OwnerId ?? request.UserId;

        var currentUser = executionContext.Current.User;
        if (!userAccessChecker.CanAccessUser(currentUser.IsAdmin, currentUser.UserId, ownerId))
        {
            return Result<CreateLoanRecordResult>.Failure(LoanErrors.ManageOwnItemRecordsOnly());
        }

        var borrowerId = request.BorrowerId;
        if (!string.IsNullOrWhiteSpace(request.BorrowerUsername))
        {
            borrowerId = await items.GetUserIdByUsernameAsync(request.BorrowerUsername, cancellationToken);
        }

        if (borrowerId <= 0)
        {
            borrowerId = null;
        }

        if (ownerId <= 0 ||
            (item?.ItemId ?? request.ItemId) <= 0 ||
            request.StartDate >= request.EndDate ||
            borrowerId is null && string.IsNullOrWhiteSpace(request.BorrowerName))
        {
            return Result<CreateLoanRecordResult>.Failure(LoanErrors.MissingCreateRecordFields());
        }

        var loan = await loans.CreateRecordAsync(
            ownerId,
            borrowerId,
            request.BorrowerName,
            item?.ItemId ?? request.ItemId,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        return loan.IsSuccess
            ? Result<CreateLoanRecordResult>.Success(Map(loan.Data!))
            : Result<CreateLoanRecordResult>.Failure(loan.Error);
    }

    private static CreateLoanRecordResult Map(UserLoan loan) => new(
        loan.OrderId,
        loan.UserId,
        loan.OrderStartDate,
        loan.OrderEndDate,
        loan.OrderStatus,
        loan.Items.Select(x => new CreateLoanRecordItemResult(
            x.ObjectDetailId,
            x.ObjectId,
            x.ObjectName,
            x.DetailStatus,
            x.ActualReturnDate)).ToArray());
}
