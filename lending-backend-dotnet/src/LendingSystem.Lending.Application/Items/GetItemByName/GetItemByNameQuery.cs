using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Items.GetItemByName;

public sealed record GetItemByNameQuery(string OwnerUsername, string ItemName) : IQuery<GetItemByNameResult>;
