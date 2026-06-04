using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.SharedKernel.Application.Abstractions;

public interface ICommand;

public interface ICommand<TResponse> : ICommand, IRequest<Result<TResponse>>;
