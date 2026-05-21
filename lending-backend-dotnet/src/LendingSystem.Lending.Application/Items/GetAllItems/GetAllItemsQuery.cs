using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

public sealed record GetAllItemsQuery : IQuery<IReadOnlyCollection<GetAllItemsResult>>;
