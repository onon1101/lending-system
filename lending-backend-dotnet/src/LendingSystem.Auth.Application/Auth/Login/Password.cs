using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Auth.Application.Auth.Login;

/// <summary>
/// 密碼型別
/// </summary>
public sealed class Password : SingleValueObject<string>
{
    private Password(string value) : base(value) { }

    public static Result<Password> Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return Result<Password>.Failure(
                new Errors("Password.Required", "密碼不可為空"));
        }
        if (password.Length < 8)
        {
            return Result<Password>.Failure(
                new Errors("Password.TooShort", "密碼長度驗證失敗"));
        }

        return new Password(password.Trim());
    }
}
