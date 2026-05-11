using LendingSystem.Application.Abstractions;
using LendingSystem.Domain.Common;
using LendingSystem.Domain.Loans;

namespace LendingSystem.Application.Loans;

public sealed class LoanService(ILoanRepository loans)
{
    public async Task<IReadOnlyCollection<UserLoanResponse>> GetUserActiveLoansAsync(int userId, CancellationToken cancellationToken)
    {
        var result = await loans.GetActiveLoansByUserIdAsync(userId, cancellationToken);
        return result.Select(Map).ToArray();
    }

    public async Task<UserLoanResponse> CreateAsync(CreateLoanRequest request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0 || request.ItemsId.Length == 0 || request.DurationHours <= 0)
        {
            throw new DomainException("Missing required fields (user_id, items_id, duration_hours)");
        }

        return Map(await loans.CreateAsync(request.UserId, request.ItemsId, request.DurationHours, cancellationToken));
    }

    public async Task<UserLoanResponse> ReturnItemAsync(int orderId, int objectId, CancellationToken cancellationToken)
    {
        if (orderId <= 0 || objectId <= 0)
        {
            throw new DomainException("Missing required fields (order_id, object_id)");
        }

        return Map(await loans.ReturnItemAsync(orderId, objectId, cancellationToken));
    }

    public async Task<IReadOnlyCollection<LoanRecordResponse>> GetHistoryByItemIdAsync(int itemId, CancellationToken cancellationToken)
    {
        var result = await loans.GetHistoryByItemIdAsync(itemId, cancellationToken);
        return result.Select(x => new LoanRecordResponse(x.StartTime, x.EndTime, x.Name, x.Status)).ToArray();
    }

    private static UserLoanResponse Map(UserLoan loan) => new(
        loan.OrderId,
        loan.UserId,
        loan.OrderStartTime,
        loan.OrderEndTime,
        loan.OrderStatus,
        loan.Items.Select(x => new LoanItemDetailResponse(x.ObjectDetailId, x.ObjectId, x.ObjectName, x.DetailStatus, x.ActualReturnTime)).ToArray());
}
