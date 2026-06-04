namespace LendingSystem.SharedKernel.Application.Common;

public sealed record RuntimeContext
{
    public string EnvironmentName { get; init; } = string.Empty;

    public bool IsDevelopment { get; init; }

    public string ApplicationName { get; init; } = string.Empty;
    
    public DateTime CurrentTime { get; } =  DateTime.UtcNow;
}