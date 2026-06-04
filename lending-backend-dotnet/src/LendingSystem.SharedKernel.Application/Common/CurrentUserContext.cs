using LendingSystem.SharedKernel.Domain.ValueObject;

namespace LendingSystem.SharedKernel.Application.Common;

public sealed record CurrentUserContext
{
    public bool IsAuthenticated { get; init; }
    
    public Guid? IdentityUserId { get; init; }
    
    public long UserId { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
    
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    
    public bool IsAdmin =>
        IsInRole(UserRole.Admin.Value);
    
    public bool IsInRole(string role) =>
        Roles.Any(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase));
}
