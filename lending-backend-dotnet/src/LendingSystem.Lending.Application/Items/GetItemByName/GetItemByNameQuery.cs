using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

public sealed record GetItemByNameQuery(string OwnerUsername, string ItemName) : IQuery<GetItemByNameResult>;
