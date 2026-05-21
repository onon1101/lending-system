using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

internal sealed class GetItemLoanHistoryQueryHandler(ILoanRepository loans) : IRequestHandler<GetItemLoanHistoryQuery, Result<IReadOnlyCollection<GetItemLoanHistoryResult>>>
{
    public async Task<Result<IReadOnlyCollection<GetItemLoanHistoryResult>>> Handle(GetItemLoanHistoryQuery request, CancellationToken cancellationToken)
    {
        var result = await loans.GetHistoryByItemIdAsync(request.ItemId, cancellationToken);
        return Result<IReadOnlyCollection<GetItemLoanHistoryResult>>.Success(result.Select(x => new GetItemLoanHistoryResult(
            x.OrderId,
            x.StartDate,
            x.EndDate,
            x.Name,
            x.Status)).ToArray());
    }
}
