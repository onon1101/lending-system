using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

public sealed record SearchUserByNameQuery(string Username) : IQuery<SearchUserByNameResult>;
