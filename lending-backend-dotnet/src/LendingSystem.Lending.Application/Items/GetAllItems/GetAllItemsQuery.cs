using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Items.GetAllItems;

public sealed record GetAllItemsQuery : IQuery<IReadOnlyCollection<GetAllItemsResult>>;
