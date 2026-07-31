namespace LendingSystem.WebApi.Configuration.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class NoPermissionRequiredAttribute : Attribute;
