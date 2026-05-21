using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Loans;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

internal sealed class CreateLoanCommandHandler(ILoanRepository loans) : IRequestHandler<CreateLoanCommand, Result<CreateLoanResult>>
{
    public async Task<Result<CreateLoanResult>> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
    {
        int? borrowerId = request.BorrowerId ?? request.UserId;
        if (borrowerId <= 0)
        {
            borrowerId = null;
        }

        if ((borrowerId is null && string.IsNullOrWhiteSpace(request.BorrowerName)) ||
            request.ItemsId.Length == 0 ||
            request.DurationDays <= 0)
        {
            return Result<CreateLoanResult>.Failure(LoanErrors.MissingCreateFields());
        }

        var loan = await loans.CreateAsync(
            borrowerId,
            request.BorrowerName,
            request.ItemsId,
            request.DurationDays,
            cancellationToken);

        return loan.IsSuccess
            ? Result<CreateLoanResult>.Success(Map(loan.Data!))
            : Result<CreateLoanResult>.Failure(loan.Error);
    }

    private static CreateLoanResult Map(UserLoan loan) => new(
        loan.OrderId,
        loan.UserId,
        loan.OrderStartDate,
        loan.OrderEndDate,
        loan.OrderStatus,
        loan.Items.Select(x => new CreateLoanItemResult(
            x.ObjectDetailId,
            x.ObjectId,
            x.ObjectName,
            x.DetailStatus,
            x.ActualReturnDate)).ToArray());
}
