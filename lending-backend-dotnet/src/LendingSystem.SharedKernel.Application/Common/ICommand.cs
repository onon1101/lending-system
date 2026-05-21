using MediatR;

namespace LendingSystem.SharedKernel.Application.Common;

public interface ICommand;

public interface ICommand<TResponse> : ICommand, IRequest<Result<TResponse>>;
