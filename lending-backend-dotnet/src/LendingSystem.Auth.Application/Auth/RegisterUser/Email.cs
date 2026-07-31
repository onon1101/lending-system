using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Auth.Application.Auth.RegisterUser;

public sealed class Email : SingleValueObject<string>
{
    private  Email(string value) : base(value)
    {}

    public static Result<Email> Create(string value)
    {
        return new Email(value.Trim());
    }
}