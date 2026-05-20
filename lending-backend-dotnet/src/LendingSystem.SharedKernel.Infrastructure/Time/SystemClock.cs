using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.SharedKernel.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
