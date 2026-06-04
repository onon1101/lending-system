using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Items.GetItemsByUserId;

public sealed record GetItemsByUserIdQuery(long UserId) : IQuery<IReadOnlyCollection<GetItemsByUserIdResult>>;
