using FluentValidation;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Lending.Application.Loans.ApproveLoanRequestCommand;

public sealed class ApproveLoanRequestCommandValidator 
    : AbstractValidator<ApproveLoanRequestCommand>
{
    public ApproveLoanRequestCommandValidator()
    {
        this.RuleFor(x => x.BorrowingKey)
            .NotEmpty()
            .WithMessage("Borrowing key is required.")
            .Must(x => PublicResourceKey.TryGetInt("borrowing", x, out var orderId) && orderId > 0)
            .WithMessage("Borrowing key is invalid.");
    }
}
