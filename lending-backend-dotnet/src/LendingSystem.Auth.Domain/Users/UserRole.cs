namespace LendingSystem.Auth.Domain.Users;

public enum UserRole 
{
    Guest = 0,
    User = 1,
    Admin = 2,
}


public static class UserRoleExtensions
{
    public static string ToString(UserRole role)
    {
        return role switch
        {
            UserRole.Guest => "Guest",
            UserRole.User => "User",
            UserRole.Admin => "Admin",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }

    public static UserRole FromString(string type)
    {
        return type.ToUpperInvariant() switch
        {
            "GUEST" => UserRole.Guest,
            "USER" => UserRole.User,
            "ADMIN" => UserRole.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
