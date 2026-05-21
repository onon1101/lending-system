using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

public sealed record GetItemMediaQuery(int ObjectId) : IQuery<IReadOnlyCollection<GetItemMediaResult>>;
