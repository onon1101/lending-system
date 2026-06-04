namespace LendingSystem.SharedKernel.Application.Common;

public sealed record ExecutionContext
{
    public required CurrentUserContext User { get; init; }
    
    public required RuntimeContext Runtime { get; init; }

    public bool IsAuthenticated => User.IsAuthenticated;
}