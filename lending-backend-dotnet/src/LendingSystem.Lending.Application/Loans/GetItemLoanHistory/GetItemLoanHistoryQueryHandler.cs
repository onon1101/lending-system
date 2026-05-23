using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

internal sealed class GetItemLoanHistoryQueryHandler(
    ILoanQueryRepository loans,
    IItemQueryRepository items) : IRequestHandler<GetItemLoanHistoryQuery, Result<IReadOnlyCollection<GetItemLoanHistoryResult>>>
{
    public async Task<Result<IReadOnlyCollection<GetItemLoanHistoryResult>>> Handle(GetItemLoanHistoryQuery request, CancellationToken cancellationToken)
    {
        var item = await items.GetByNameAsync(request.OwnerUsername, Uri.UnescapeDataString(request.ObjectName.Trim()), cancellationToken);
        if (item is null)
        {
            return Result<IReadOnlyCollection<GetItemLoanHistoryResult>>.Success([]);
        }

        var result = await loans.GetHistoryByItemIdAsync(item.ItemId, cancellationToken);
        return Result<IReadOnlyCollection<GetItemLoanHistoryResult>>.Success(result.Select(x => new GetItemLoanHistoryResult(
            x.OrderId,
            x.StartDate,
            x.EndDate,
            x.Name,
            x.Status)).ToArray());
    }
}
