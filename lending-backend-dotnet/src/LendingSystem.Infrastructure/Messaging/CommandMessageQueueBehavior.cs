using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Infrastructure.Messaging;

public sealed class CommandMessageQueueBehavior<TRequest, TResponse>(IMessageQueue queue)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var result = await next(cancellationToken);
        if (request is ICommand && IsSuccessResult(result))
        {
            await queue.PublishAsync(
                $"commands.{typeof(TRequest).Name}",
                new CommandCompletedMessage<TRequest, TResponse>(request, result, DateTimeOffset.UtcNow),
                cancellationToken);
        }

        return result;
    }

    private static bool IsSuccessResult(TResponse response) =>
        response?.GetType().GetProperty("IsSuccess")?.GetValue(response) is true;
}

public sealed record CommandCompletedMessage<TCommand, TResult>(
    TCommand Command,
    TResult Result,
    DateTimeOffset OccurredAt);
