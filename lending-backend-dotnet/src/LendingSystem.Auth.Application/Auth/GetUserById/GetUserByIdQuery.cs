using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

public sealed record GetUserByIdQuery(long UserId) : IQuery<GetUserByIdResult>;
