using Microsoft.AspNetCore.Authorization;

namespace LendingSystem.WebApi.Configuration.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyName = "HasPermission";

    public HasPermissionAttribute(string name)
        : base(PolicyName)
    {
        Name = name;
    }

    public string Name { get; }
}
