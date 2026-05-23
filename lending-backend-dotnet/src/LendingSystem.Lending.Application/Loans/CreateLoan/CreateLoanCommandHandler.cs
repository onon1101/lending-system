using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Aggregate.Loans;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

internal sealed class CreateLoanCommandHandler(
    ILoanCommandRepository loans,
    IItemQueryRepository items,
    IExecutionContextAccessor executionContext) : IRequestHandler<CreateLoanCommand, Result<CreateLoanResult>>
{
    public async Task<Result<CreateLoanResult>> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
    {
        long? borrowerId = request.BorrowerId ?? request.UserId;
        if (!string.IsNullOrWhiteSpace(request.BorrowerUsername))
        {
            borrowerId = await items.GetUserIdByUsernameAsync(request.BorrowerUsername, cancellationToken);
        }

        if (!executionContext.CanAccessUser(borrowerId ?? 0))
        {
            return Result<CreateLoanResult>.Failure(LoanErrors.CreateBorrowingsForSelfOnly());
        }

        if (borrowerId <= 0)
        {
            borrowerId = null;
        }

        if ((borrowerId is null && string.IsNullOrWhiteSpace(request.BorrowerName)) ||
            request.Items.Length == 0 && request.ItemsId.Length == 0 ||
            request.DurationDays <= 0)
        {
            return Result<CreateLoanResult>.Failure(LoanErrors.MissingCreateFields());
        }

        var itemIds = request.ItemsId.ToList();
        foreach (var itemRequest in request.Items)
        {
            var item = await items.GetByNameAsync(itemRequest.OwnerUsername, itemRequest.ObjectName, cancellationToken);
            if (item is null)
            {
                return Result<CreateLoanResult>.Failure(LoanErrors.MissingCreateFields());
            }

            itemIds.Add(item.ItemId);
        }

        var loan = await loans.CreateAsync(
            borrowerId,
            request.BorrowerName,
            itemIds,
            request.DurationDays,
            cancellationToken);

        return loan.IsSuccess
            ? Result<CreateLoanResult>.Success(Map(loan.Data!))
            : Result<CreateLoanResult>.Failure(loan.Error);
    }

    private static CreateLoanResult Map(UserLoan loan) => new(
        loan.OrderId,
        loan.UserId,
        loan.OrderStartDate,
        loan.OrderEndDate,
        loan.OrderStatus,
        loan.Items.Select(x => new CreateLoanItemResult(
            x.ObjectDetailId,
            x.ObjectId,
            x.ObjectName,
            x.DetailStatus,
            x.ActualReturnDate)).ToArray());
}
