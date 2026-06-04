using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Auth.Application.Auth.GetUserById;

public sealed record GetUserByIdQuery(long UserId) : IQuery<GetUserByIdResult>;
