using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Auth.Application.Auth.GetUserByName;

public sealed record GetUserByNameQuery(string Username) : IQuery<GetUserByNameResult>;