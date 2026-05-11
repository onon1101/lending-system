using LendingSystem.Application.Abstractions;

namespace LendingSystem.Application.System;

public interface IDatabaseHealthCheck
{
    Task<string?> GetErrorAsync(CancellationToken cancellationToken);
}

public sealed class SystemStatusService(IClock clock, IDatabaseHealthCheck database)
{
    public ServiceHealthResponse GetHealth() => new("ok", "lending-backend", clock.UtcNow);

    public async Task<SystemStatusResponse> GetStatusAsync(CancellationToken cancellationToken)
    {
        var error = await database.GetErrorAsync(cancellationToken);
        return error is null
            ? new SystemStatusResponse("ok", "ok", new DependencyStatus("ok"), clock.UtcNow)
            : new SystemStatusResponse("degraded", "ok", new DependencyStatus("error", error), clock.UtcNow);
    }
}
