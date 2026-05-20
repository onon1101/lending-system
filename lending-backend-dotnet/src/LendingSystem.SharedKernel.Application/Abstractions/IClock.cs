namespace LendingSystem.SharedKernel.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
