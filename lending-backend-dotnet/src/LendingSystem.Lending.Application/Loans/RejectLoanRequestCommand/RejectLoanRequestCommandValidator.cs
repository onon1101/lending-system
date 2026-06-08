using FluentValidation;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Lending.Application.Loans.RejectLoanRequestCommand;

public sealed class RejectLoanRequestCommandValidator
    : AbstractValidator<RejectLoanRequestCommand>
{
    public RejectLoanRequestCommandValidator()
    {
        this.RuleFor(x => x.BorrowingKey)
            .NotEmpty()
            .WithMessage("Borrowing key is required.")
            .Must(x => PublicResourceKey.TryGetInt("borrowing", x, out var orderId) && orderId > 0)
            .WithMessage("Borrowing key is invalid.");
    }
}
