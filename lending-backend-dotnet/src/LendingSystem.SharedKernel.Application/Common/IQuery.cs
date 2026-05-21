using MediatR;

namespace LendingSystem.SharedKernel.Application.Common;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
