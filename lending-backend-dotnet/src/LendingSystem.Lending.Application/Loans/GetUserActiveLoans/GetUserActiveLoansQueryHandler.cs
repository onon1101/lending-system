using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Loans;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

internal sealed class GetUserActiveLoansQueryHandler(ILoanRepository loans) : IRequestHandler<GetUserActiveLoansQuery, Result<IReadOnlyCollection<GetUserActiveLoansResult>>>
{
    public async Task<Result<IReadOnlyCollection<GetUserActiveLoansResult>>> Handle(GetUserActiveLoansQuery request, CancellationToken cancellationToken)
    {
        var result = await loans.GetActiveLoansByUserIdAsync(request.UserId, cancellationToken);
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
