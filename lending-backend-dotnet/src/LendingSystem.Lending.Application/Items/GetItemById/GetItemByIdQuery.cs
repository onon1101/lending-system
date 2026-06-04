using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Items.GetItemById;

public sealed record GetItemByIdQuery(long ItemId) : IQuery<GetItemByIdResult>;
