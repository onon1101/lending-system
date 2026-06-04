using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Auth.Domain.ValueObjects;

public sealed class UserRole : ValueObject
{
    public static readonly UserRole Guest = new("Guest");
    public static readonly UserRole User = new("User");
    public static readonly UserRole Admin = new("Admin");

    private UserRole(string value)
    {
        Value = value;
    }

    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static UserRole FromString(string? value)
    {
        return value?.Trim().ToUpperInvariant() switch
        {
            null or "" or "USER" => User,
            "GUEST" => Guest,
            "ADMIN" => Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported user role.")
        };
    }

    public override string ToString() => Value;
}
