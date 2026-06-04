using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Items.GetItemMedia;

public sealed record GetItemMediaQuery(string OwnerUsername, string ObjectName) : IQuery<IReadOnlyCollection<GetItemMediaResult>>;
