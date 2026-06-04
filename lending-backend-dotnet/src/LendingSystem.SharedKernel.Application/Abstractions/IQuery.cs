using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.SharedKernel.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
