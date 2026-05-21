using System.Collections.Concurrent;
using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.SharedKernel.Infrastructure.Messaging;

public sealed class InMemoryMessageQueue : IMessageQueue
{
    private readonly ConcurrentQueue<QueuedMessage> messages = new();

    public Task PublishAsync(string topic, object message, CancellationToken cancellationToken)
    {
        messages.Enqueue(new QueuedMessage(topic, message, DateTimeOffset.UtcNow));
        return Task.CompletedTask;
    }

    public IReadOnlyCollection<QueuedMessage> Messages => messages.ToArray();
}

public sealed record QueuedMessage(string Topic, object Message, DateTimeOffset PublishedAt);
