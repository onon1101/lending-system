using FluentValidation;

namespace LendingSystem.Auth.Application.Auth.GetUserByName;

public sealed class GetUserByNameValidator : AbstractValidator<GetUserByNameQuery>
{
    public GetUserByNameValidator()
    {
        this.RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username cannot be empty");
    }
}