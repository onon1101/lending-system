using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Auth.Application.Auth.SearchUserByName;

public sealed record SearchUserByNameQuery(string Username) : IQuery<SearchUserByNameResult>;
