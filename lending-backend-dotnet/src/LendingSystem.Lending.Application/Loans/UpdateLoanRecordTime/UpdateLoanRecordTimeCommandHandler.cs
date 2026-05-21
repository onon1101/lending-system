using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Loans;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

internal sealed class UpdateLoanRecordTimeCommandHandler(ILoanRepository loans) : IRequestHandler<UpdateLoanRecordTimeCommand, Result<UpdateLoanRecordTimeResult>>
{
    public async Task<Result<UpdateLoanRecordTimeResult>> Handle(UpdateLoanRecordTimeCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0 ||
            request.OrderId <= 0 ||
            request.StartDate is null && request.EndDate is null)
        {
            return Result<UpdateLoanRecordTimeResult>.Failure(LoanErrors.MissingUpdateRecordTimeFields());
        }

        if (request.StartDate is not null &&
            request.EndDate is not null &&
            request.StartDate >= request.EndDate)
        {
            return Result<UpdateLoanRecordTimeResult>.Failure(LoanDomainError.StartDateMustBeEarlierThanEndDate());
        }

        var loan = await loans.UpdateRecordTimeAsync(
            request.UserId,
            request.OrderId,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        return loan.IsSuccess
            ? Result<UpdateLoanRecordTimeResult>.Success(Map(loan.Data!))
            : Result<UpdateLoanRecordTimeResult>.Failure(loan.Error);
    }

    private static UpdateLoanRecordTimeResult Map(UserLoan loan) => new(
        loan.OrderId,
        loan.UserId,
        loan.OrderStartDate,
        loan.OrderEndDate,
        loan.OrderStatus,
        loan.Items.Select(x => new UpdateLoanRecordTimeItemResult(
            x.ObjectDetailId,
            x.ObjectId,
            x.ObjectName,
            x.DetailStatus,
            x.ActualReturnDate)).ToArray());
}
