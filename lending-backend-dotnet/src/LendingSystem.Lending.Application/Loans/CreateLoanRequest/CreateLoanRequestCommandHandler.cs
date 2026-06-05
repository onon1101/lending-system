using FluentValidation;
using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Aggregates.Loans;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans.CreateLoanRequest;

public class CreateLoanRequestCommandHandler(
    ILoanCommandRepository loans,
    IExecutionContextAccessor executionContext,
    IClock clock,
    IValidator<CreateLoanRequestCommand> validator)
:IRequestHandler<CreateLoanRequestCommand, Result<CreateLoanRequestResult>>
{
    public async Task<Result<CreateLoanRequestResult>> Handle(CreateLoanRequestCommand request, CancellationToken cancellationToken)
    {
        // 基本欄位值的驗證
        var validation = await validator.ValidateAsync(request, cancellationToken);
        var currentUserId = executionContext.Current.User.UserId;

        if (!validation.IsValid || currentUserId <= 0)
        {
            return Result<CreateLoanRequestResult>.Failure(LoanErrors.ValidateFieldError());
        }

        // 時間週期的 Value Object
        var period = LoanPeriod.Create(request.StartDate, request.DurationDays);
        if (!period.IsSuccess)
        {
            return Result<CreateLoanRequestResult>.Failure(period.Error);
        }

        // 取得當前借閱者
        var borrower = await loans.GetActiveRequestUserAsync(currentUserId, cancellationToken);
        if (borrower is null)
        {
            return Result<CreateLoanRequestResult>.Failure(LoanErrors.BorrowerNotFound(currentUserId));
        }

        // 取得欲借閱物品
        var itemOwnerUsername = request.ItemOwnerUsername.Trim();
        var itemName = request.ItemName.Trim();
        var item = await loans.GetRequestItemAsync(itemOwnerUsername, itemName, cancellationToken);
        if (item is null)
        {
            return Result<CreateLoanRequestResult>.Failure(LoanErrors.ItemOwnerOrItemNotFound(itemOwnerUsername, itemName));
        }

        // 創建借閱請求
        var borrowerDetail = await loans.PrepareBorrowerDetailReferenceAsync(
                borrower.UserId,
                borrower.Name,
                item.OwnerId,
                Today(),
                cancellationToken);
        var aggregate = LoansAggregate.RequestBorrowing(
            borrowerDetail.BorrowerDetailId,
            item.OwnerId,
            item.OwnerName,
            borrower.UserId,
            borrower.Name,
            item.ItemId,
            item.ItemName,
            item.CurrentStatus,
            period.Data!);
        if (!aggregate.IsSuccess)
        {
            return Result<CreateLoanRequestResult>.Failure(aggregate.Error);
        }

        // 儲存變更
        var requestAggregate = aggregate.Data!;
        var loan = await loans.SaveRequestAsync(
            requestAggregate,
            borrowerDetail,
            Today(),
            cancellationToken);

        if (!loan.IsSuccess)
        {
            return Result<CreateLoanRequestResult>.Failure(loan.Error);
        }

        return Result<CreateLoanRequestResult>.Success(
            new CreateLoanRequestResult("建立請求以送出"));
    }

    private DateOnly Today() => DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
}
