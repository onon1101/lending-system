using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace LendingSystem.Auth.Application.Auth;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator(EmailAddressAttribute emailAddressAttribute)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email cannot be empty")
            .Must(email => IsValidEmail(email, emailAddressAttribute))
            .WithMessage("Invalid email");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password cannot be empty");
    }

    private static bool IsValidEmail(string email, EmailAddressAttribute emailAddressAttribute)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        if (!emailAddressAttribute.IsValid(email))
        {
            return false;
        }

        var parts = email.Split('@');
        if (parts.Length != 2)
        {
            return false;
        }

        var domainParts = parts[1].Split('.');
        return domainParts.Length >= 2 &&
            domainParts.All(p => !string.IsNullOrWhiteSpace(p)) &&
            domainParts[^1].Length >= 2;
    }
}
