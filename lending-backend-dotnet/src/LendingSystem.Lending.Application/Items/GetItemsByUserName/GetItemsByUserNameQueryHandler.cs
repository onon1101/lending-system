using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Aggregate.Item;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

internal sealed class GetItemsByUserNameQueryHandler(IItemQueryRepository items) : IRequestHandler<GetItemsByUserNameQuery, Result<IReadOnlyCollection<GetItemsByUserNameResult>>>
{
    public async Task<Result<IReadOnlyCollection<GetItemsByUserNameResult>>> Handle(GetItemsByUserNameQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return Result<IReadOnlyCollection<GetItemsByUserNameResult>>.Failure(ItemErrors.UsernameRequired());
        }

        var result = await items.GetItemsByUserName(Uri.UnescapeDataString(request.Username.Trim()), cancellationToken);
        return result is null
            ? Result<IReadOnlyCollection<GetItemsByUserNameResult>>.Failure(ItemErrors.UserNotFound())
            : Result<IReadOnlyCollection<GetItemsByUserNameResult>>.Success(result.Select(Map).ToArray());
    }

    private static GetItemsByUserNameResult Map(ItemSummary item) => new(
        item.ItemId,
        item.OwnerId,
        item.ObjectName,
        item.Maker,
        item.Material,
        item.Description,
        item.CurrentStatus,
        item.OwnerUsername,
        item.OwnerName,
        item.OwnerEmail,
        item.ImageUrl);
}
