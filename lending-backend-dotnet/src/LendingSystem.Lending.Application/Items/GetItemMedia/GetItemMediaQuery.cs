using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Items;

public sealed record GetItemMediaQuery(string OwnerUsername, string ObjectName) : IQuery<IReadOnlyCollection<GetItemMediaResult>>;
