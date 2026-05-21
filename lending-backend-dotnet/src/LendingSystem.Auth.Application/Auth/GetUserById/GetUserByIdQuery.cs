using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

public sealed record GetUserByIdQuery(int UserId) : IQuery<GetUserByIdResult>;
