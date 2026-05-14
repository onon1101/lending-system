using LendingSystem.Application.Abstractions;
using LendingSystem.Application.Common;
using LendingSystem.Domain.Loans;

namespace LendingSystem.Application.Loans;

public sealed class LoanService(ILoanRepository loans)
{
    public async Task<Result<IReadOnlyCollection<UserLoanResponse>>> GetUserActiveLoansAsync(int userId, CancellationToken cancellationToken)
    {
        var result = await loans.GetActiveLoansByUserIdAsync(userId, cancellationToken);
        return Result<IReadOnlyCollection<UserLoanResponse>>.Success(result.Select(Map).ToArray());
    }

    public async Task<Result<UserLoanResponse>> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken)
    {
        int? borrowerId = request.BorrowerId ?? request.UserId;
        if (borrowerId <= 0)
        {
            borrowerId = null;
        }

        if ((borrowerId is null && string.IsNullOrWhiteSpace(request.BorrowerName)) || request.ItemsId.Length == 0 || request.DurationDays <= 0)
        {
            return Result<UserLoanResponse>.Failure(ErrorCodes.Validation, "Missing required fields (borrower_id or borrower_name, items_id, duration_days)");
        }

        var loan = await loans.CreateAsync(borrowerId, request.BorrowerName, request.ItemsId, request.DurationDays, cancellationToken);
        return loan.IsSuccess
            ? Result<UserLoanResponse>.Success(Map(loan.Data!))
            : Result<UserLoanResponse>.Failure(loan.Error.Code, loan.Error.Message);
    }

    public async Task<Result<UserLoanResponse>> ReturnItemAsync(int orderId, int objectId, CancellationToken cancellationToken)
    {
        if (orderId <= 0 || objectId <= 0)
        {
            return Result<UserLoanResponse>.Failure(ErrorCodes.Validation, "Missing required fields (order_id, object_id)");
        }

        var loan = await loans.ReturnItemAsync(orderId, objectId, cancellationToken);
        return loan.IsSuccess
            ? Result<UserLoanResponse>.Success(Map(loan.Data!))
            : Result<UserLoanResponse>.Failure(loan.Error.Code, loan.Error.Message);
    }

    public async Task<Result<UserLoanResponse>> CreateRecordAsync(CreateRecordRequest request,
        CancellationToken cancellationToken)
    {
        var borrowerId = request.BorrowerId;
        if (borrowerId <= 0)
        {
            borrowerId = null;
        }

        if (request.UserId <= 0 || request.ItemId <= 0 || request.StartDate >= request.EndDate ||
            (borrowerId is null && string.IsNullOrWhiteSpace(request.BorrowerName)))
        {
            return Result<UserLoanResponse>.Failure(ErrorCodes.Validation, "Missing required fields (user_id, borrower_id or borrower_name, item_id, start_date, end_date)");
        }

        var loan = await loans.CreateRecordAsync(request.UserId, borrowerId, request.BorrowerName, request.ItemId, request.StartDate, request.EndDate, cancellationToken);
        return loan.IsSuccess
            ? Result<UserLoanResponse>.Success(Map(loan.Data!))
            : Result<UserLoanResponse>.Failure(loan.Error.Code, loan.Error.Message);
    }

    public async Task<Result<DeleteLoanRecordResponse>> DeleteRecordAsync(int ownerId, int orderId, CancellationToken cancellationToken)
    {
        if (ownerId <= 0 || orderId <= 0)
        {
            return Result<DeleteLoanRecordResponse>.Failure(ErrorCodes.Validation, "Missing required fields (user_id, order_id)");
        }

        var deleted = await loans.DeleteRecordAsync(ownerId, orderId, cancellationToken);
        return deleted.IsSuccess
            ? Result<DeleteLoanRecordResponse>.Success(new DeleteLoanRecordResponse(true, $"Delete borrowing record from order_id {orderId} is successful."))
            : Result<DeleteLoanRecordResponse>.Failure(deleted.Error.Code, deleted.Error.Message);
    }

    public async Task<Result<UserLoanResponse>> UpdateRecordTimeAsync(int orderId, UpdateRecordTimeRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0 || orderId <= 0 || (request.StartDate is null && request.EndDate is null))
        {
            return Result<UserLoanResponse>.Failure(ErrorCodes.Validation, "Missing required fields (user_id, order_id, start_date or end_date)");
        }

        if (request.StartDate is not null && request.EndDate is not null && request.StartDate >= request.EndDate)
        {
            return Result<UserLoanResponse>.Failure(LoanDomainError.StartDateMustBeEarlierThanEndDate());
        }

        var loan = await loans.UpdateRecordTimeAsync(request.UserId, orderId, request.StartDate, request.EndDate, cancellationToken);
        return loan.IsSuccess
            ? Result<UserLoanResponse>.Success(Map(loan.Data!))
            : Result<UserLoanResponse>.Failure(loan.Error.Code, loan.Error.Message);
    }

    public async Task<Result<IReadOnlyCollection<LoanRecordResponse>>> GetHistoryByItemIdAsync(int itemId, CancellationToken cancellationToken)
    {
        var result = await loans.GetHistoryByItemIdAsync(itemId, cancellationToken);
        return Result<IReadOnlyCollection<LoanRecordResponse>>.Success(result.Select(x => new LoanRecordResponse(x.OrderId, x.StartDate, x.EndDate, x.Name, x.Status)).ToArray());
    }

    private static UserLoanResponse Map(UserLoan loan) => new(
        loan.OrderId,
        loan.UserId,
        loan.OrderStartDate,
        loan.OrderEndDate,
        loan.OrderStatus,
        loan.Items.Select(x => new LoanItemDetailResponse(x.ObjectDetailId, x.ObjectId, x.ObjectName, x.DetailStatus, x.ActualReturnDate)).ToArray());
}
