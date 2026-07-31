using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Auth.Application.Auth.Login;

/// <summary>
/// 使用者登入帳號
/// </summary>
public sealed class Account : SingleValueObject<string>
{
    private Account(string value) : base(value) { }

    public static Result<Account> Create(string account)
    {
        if (string.IsNullOrWhiteSpace(account))
        {
            return Result<Account>.Failure(
                new Errors("Account.Required", "帳號不可為空"));
        }

        return new Account(account.Trim());
    }
}