namespace LendingSystem.Domain.Users;

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
        switch (role)
        {
            case UserRole.Guest:
                return "Guest";
            case UserRole.User:
                return "User";
            case UserRole.Admin:
                return "Admin";
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }
    }

    public static UserRole FromString(string type)
    {
        switch (type)
        {
            case "Guest":
                return UserRole.Guest;
            case "User":
                return UserRole.User;
            case "Admin":
                return UserRole.Admin;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}