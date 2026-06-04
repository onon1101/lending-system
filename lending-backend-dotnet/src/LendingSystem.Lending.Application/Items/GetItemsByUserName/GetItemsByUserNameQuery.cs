using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Items.GetItemsByUserName;

public sealed record GetItemsByUserNameQuery(string Username) : IQuery<IReadOnlyCollection<GetItemsByUserNameResult>>;
