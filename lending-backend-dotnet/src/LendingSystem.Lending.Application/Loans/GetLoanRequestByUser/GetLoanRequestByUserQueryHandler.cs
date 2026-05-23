using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Aggregate.Loans;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans.GetLoanRequestByUser;

internal sealed class GetLoanRequestByUserQueryHandler(
    ILoanQueryRepository loans,
    IExecutionContextAccessor executionContext) : IRequestHandler<GetLoanRequestByUserQuery, Result<IReadOnlyCollection<GetLoanRequestByUserResult>>>
{
    /// <summary>
    /// 取得當前使用者所有被借閱請求
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result<IReadOnlyCollection<GetLoanRequestByUserResult>>> Handle(GetLoanRequestByUserQuery request, CancellationToken cancellationToken)
    {
        if (executionContext.CurrentUserId <= 0)
        {
            return Result<IReadOnlyCollection<GetLoanRequestByUserResult>>.Success([]);
        }

        var loanRequests = await loans.GetRequestsByOwnerIdAsync(executionContext.CurrentUserId, cancellationToken);
        return Result<IReadOnlyCollection<GetLoanRequestByUserResult>>.Success(
            loanRequests.Select(Map).ToArray());
    }

    private static GetLoanRequestByUserResult Map(LoanRequestRecord loanRequest) => new(
        loanRequest.OrderId,
        loanRequest.ItemName,
        loanRequest.BorrowerName,
        loanRequest.BorrowerUsername,
        loanRequest.StartDate,
        loanRequest.EndDate,
        loanRequest.Status);
}
