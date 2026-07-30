namespace LendingSystem.SharedKernel.Domain.Common;

public sealed record Errors(
    string Code,
    string ErrorMessage,
    ErrorType Type = ErrorType.Validation)
{
    public static Errors None { get; } =
        new(string.Empty, string.Empty, ErrorType.None);
}
