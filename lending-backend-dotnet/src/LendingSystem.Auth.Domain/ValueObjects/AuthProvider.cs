using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Auth.Domain.ValueObjects;

public sealed class AuthProvider : ValueObject
{
    public static readonly AuthProvider Local = new("LOCAL");
    public static readonly AuthProvider Google = new("GOOGLE");

    private AuthProvider(string value)
    {
        Value = value;
    }

    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static AuthProvider FromString(string? value)
    {
        return value?.Trim().ToUpperInvariant() switch
        {
            null or "" or "LOCAL" => Local,
            "GOOGLE" => Google,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported auth provider.")
        };
    }

    public override string ToString() => Value;
}
