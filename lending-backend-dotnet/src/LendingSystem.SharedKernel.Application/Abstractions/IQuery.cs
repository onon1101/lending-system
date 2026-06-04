using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.SharedKernel.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
