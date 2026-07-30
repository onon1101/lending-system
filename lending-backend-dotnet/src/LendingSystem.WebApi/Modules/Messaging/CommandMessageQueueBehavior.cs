using LendingSystem.SharedKernel.Application.Abstractions;
using MediatR;
using DomainResult = LendingSystem.SharedKernel.Domain.Common.IResult;

namespace LendingSystem.WebApi.Modules.Messaging;

public sealed class CommandMessageQueueBehavior<TRequest, TResponse>(
    IMessageQueue queue,
    IClock clock)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var result = await next(cancellationToken);

        if (request is not ICommand || result is not DomainResult { IsSuccess: true })
        {
            return result;
        }

        var topic = $"commands.{typeof(TRequest).Name}";
        var message = new CommandCompletedMessage<TRequest, TResponse>(
            request,
            result,
            clock.UtcNow);

        await queue.PublishAsync(topic, message, cancellationToken);

        return result;
    }
}

public sealed record CommandCompletedMessage<TCommand, TResult>(
    TCommand Command,
    TResult Result,
    DateTimeOffset OccurredAt);
