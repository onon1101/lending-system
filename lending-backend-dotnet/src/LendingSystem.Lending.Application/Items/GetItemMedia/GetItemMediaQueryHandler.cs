using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items.GetItemMedia;

internal sealed class GetItemMediaQueryHandler(IItemQueryRepository items) : IRequestHandler<GetItemMediaQuery, Result<IReadOnlyCollection<GetItemMediaResult>>>
{
    public async Task<Result<IReadOnlyCollection<GetItemMediaResult>>> Handle(GetItemMediaQuery request, CancellationToken cancellationToken)
    {
        var item = await items.GetByNameAsync(request.OwnerUsername, Uri.UnescapeDataString(request.ObjectName.Trim()), cancellationToken);
        if (item is null)
        {
            return Result<IReadOnlyCollection<GetItemMediaResult>>.Success([]);
        }

        var result = await items.GetMediaByItemIdAsync(item.ItemId, cancellationToken);
        return Result<IReadOnlyCollection<GetItemMediaResult>>.Success(result.Select(x => new GetItemMediaResult(
            x.Type,
            x.Creator,
            x.Description,
            x.OriginalLink,
            x.Media,
            x.CreatedAt)).ToArray());
    }
}
