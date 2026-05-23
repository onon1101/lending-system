using FluentValidation;
using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans.CreateLoanRequest;

public class CreateLoanRequestCommandHandler(
    ILoanCommandRepository loans,
    IExecutionContextAccessor executionContext,
    IValidator<CreateLoanRequestCommand> validator)
:IRequestHandler<CreateLoanRequestCommand, Result<CreateLoanRequestResult>>
{
    public async Task<Result<CreateLoanRequestResult>> Handle(CreateLoanRequestCommand request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid || executionContext.CurrentUserId <= 0)
        {
            return Result<CreateLoanRequestResult>.Failure(LoanErrors.ValidateFieldError());
        }

        var loan = await loans.CreateRequestAsync(
            executionContext.CurrentUserId,
            request.BorrowerName.Trim(),
            request.ItemName.Trim(),
            request.StartDate,
            request.DurationDays,
            cancellationToken);

        if (!loan.IsSuccess)
        {
            return Result<CreateLoanRequestResult>.Failure(loan.Error);
        }

        return Result<CreateLoanRequestResult>.Success(new CreateLoanRequestResult("建立請求以送出"));
    }
}
