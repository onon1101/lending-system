using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

public sealed record GetItemsByUserIdQuery(int UserId) : IQuery<IReadOnlyCollection<GetItemsByUserIdResult>>;
