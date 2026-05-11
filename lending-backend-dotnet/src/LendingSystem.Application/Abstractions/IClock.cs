namespace LendingSystem.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
