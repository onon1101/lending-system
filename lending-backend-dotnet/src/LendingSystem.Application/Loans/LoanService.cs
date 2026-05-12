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
        var borrowerId = request.BorrowerId ?? request.UserId;
        if (borrowerId <= 0)
        {
            borrowerId = null;
        }

        if ((borrowerId is null && string.IsNullOrWhiteSpace(request.BorrowerName)) || request.ItemsId.Length == 0 || request.DurationHours <= 0)
        {
            return Result<UserLoanResponse>.Failure(ErrorCodes.Validation, "Missing required fields (borrower_id or borrower_name, items_id, duration_hours)");
        }

        var loan = await loans.CreateAsync(borrowerId, request.BorrowerName, request.ItemsId, request.DurationHours, cancellationToken);
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

    public async Task<Result<IReadOnlyCollection<LoanRecordResponse>>> GetHistoryByItemIdAsync(int itemId, CancellationToken cancellationToken)
    {
        var result = await loans.GetHistoryByItemIdAsync(itemId, cancellationToken);
        return Result<IReadOnlyCollection<LoanRecordResponse>>.Success(result.Select(x => new LoanRecordResponse(x.StartTime, x.EndTime, x.Name, x.Status)).ToArray());
    }

    private static UserLoanResponse Map(UserLoan loan) => new(
        loan.OrderId,
        loan.UserId,
        loan.OrderStartTime,
        loan.OrderEndTime,
        loan.OrderStatus,
        loan.Items.Select(x => new LoanItemDetailResponse(x.ObjectDetailId, x.ObjectId, x.ObjectName, x.DetailStatus, x.ActualReturnTime)).ToArray());
}
