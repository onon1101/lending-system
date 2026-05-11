using LendingSystem.Application.Abstractions;

namespace LendingSystem.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
