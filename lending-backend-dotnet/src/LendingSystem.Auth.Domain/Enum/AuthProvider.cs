namespace LendingSystem.Auth.Domain.Enum;

public enum AuthProvider
{
    Google = 0,
    Local,
}

public static class AuthProviderExtensions
{
    public static string ToString(AuthProvider provider)
    {
        switch (provider)
        {
            case AuthProvider.Local:
                return "LOCAL";
            case AuthProvider.Google:
                return "GOOGLE";
            default:
                throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
        }
    }

    public static AuthProvider FromString(string type)
    {
        switch (type.ToUpperInvariant())
        {
            case "LOCAL":
                return AuthProvider.Local;
            case "GOOGLE":
                return AuthProvider.Google;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}
