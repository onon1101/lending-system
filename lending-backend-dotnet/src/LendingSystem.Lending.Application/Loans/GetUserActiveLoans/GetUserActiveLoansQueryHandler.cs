using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Aggregate.Loans;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

internal sealed class GetUserActiveLoansQueryHandler(
    ILoanQueryRepository loans,
    IItemQueryRepository items,
    IExecutionContextAccessor executionContext) : IRequestHandler<GetUserActiveLoansQuery, Result<IReadOnlyCollection<GetUserActiveLoansResult>>>
{
    public async Task<Result<IReadOnlyCollection<GetUserActiveLoansResult>>> Handle(GetUserActiveLoansQuery request, CancellationToken cancellationToken)
    {
        var userId = await items.GetUserIdByUsernameAsync(request.Username, cancellationToken);
        if (userId is null)
        {
            return Result<IReadOnlyCollection<GetUserActiveLoansResult>>.Success([]);
        }

        if (!executionContext.CanAccessUser(userId.Value))
        {
            return Result<IReadOnlyCollection<GetUserActiveLoansResult>>.Failure(LoanErrors.AccessOwnBorrowingsOnly());
        }

        var result = await loans.GetActiveLoansByUserIdAsync(userId.Value, cancellationToken);
        return Result<IReadOnlyCollection<GetUserActiveLoansResult>>.Success(result.Select(Map).ToArray());
    }

    private static GetUserActiveLoansResult Map(UserLoan loan) => new(
        loan.OrderId,
        loan.UserId,
        loan.OrderStartDate,
        loan.OrderEndDate,
        loan.OrderStatus,
        loan.Items.Select(x => new GetUserActiveLoansItemResult(
            x.ObjectDetailId,
            x.ObjectId,
            x.ObjectName,
            x.DetailStatus,
            x.ActualReturnDate)).ToArray());
}
