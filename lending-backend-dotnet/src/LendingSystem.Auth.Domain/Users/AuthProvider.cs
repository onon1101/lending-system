namespace LendingSystem.Auth.Domain.Users;

public enum AuthProvider
{
    Google = 0,
    Local,
}

public static class AuthProviderExtensions
{
    public static string ToString(AuthProvider provider)
    {
        return provider switch
        {
            AuthProvider.Local => "LOCAL",
            AuthProvider.Google => "GOOGLE",
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }

    public static AuthProvider FromString(string type)
    {
        return type.ToUpperInvariant() switch
        {
            "LOCAL" => AuthProvider.Local,
            "GOOGLE" => AuthProvider.Google,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
