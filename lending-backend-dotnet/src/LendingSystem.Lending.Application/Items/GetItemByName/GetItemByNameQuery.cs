using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

public sealed record GetItemByNameQuery(int UserId, string ItemName) : IQuery<GetItemByNameResult>;
