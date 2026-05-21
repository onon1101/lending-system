using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

internal sealed class GetItemMediaQueryHandler(IItemRepository items) : IRequestHandler<GetItemMediaQuery, Result<IReadOnlyCollection<GetItemMediaResult>>>
{
    public async Task<Result<IReadOnlyCollection<GetItemMediaResult>>> Handle(GetItemMediaQuery request, CancellationToken cancellationToken)
    {
        var result = await items.GetMediaByItemIdAsync(request.ObjectId, cancellationToken);
        return Result<IReadOnlyCollection<GetItemMediaResult>>.Success(result.Select(x => new GetItemMediaResult(
            x.Type,
            x.Creator,
            x.Description,
            x.OriginalLink,
            x.Media,
            x.CreatedAt)).ToArray());
    }
}
