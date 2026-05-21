namespace LendingSystem.SharedKernel.Application.Abstractions;

public interface IMessageQueue
{
    Task PublishAsync(string topic, object message, CancellationToken cancellationToken);
}
