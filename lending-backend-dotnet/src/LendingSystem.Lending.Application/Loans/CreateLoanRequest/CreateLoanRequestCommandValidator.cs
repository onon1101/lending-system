using FluentValidation;

namespace LendingSystem.Lending.Application.Loans.CreateLoanRequest;

public class CreateLoanRequestCommandValidator : AbstractValidator<CreateLoanRequestCommand>
{
    public CreateLoanRequestCommandValidator()
    {
        this.RuleFor(x => x.ItemOwnerUsername)
            .NotEmpty()
            .WithMessage("ItemOwnerUsername is required");
        
        this.RuleFor(x => x.ItemName)
            .NotEmpty()
            .WithMessage("ItemName is required");
        
        this.RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("Start Date is required");
        
        this.RuleFor(x => x.DurationDays)
            .GreaterThan(0)
            .WithMessage("DurationDays must be greater than 0");
    }
}
