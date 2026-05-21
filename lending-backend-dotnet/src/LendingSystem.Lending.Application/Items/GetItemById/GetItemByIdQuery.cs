using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

public sealed record GetItemByIdQuery(int ItemId) : IQuery<GetItemByIdResult>;
