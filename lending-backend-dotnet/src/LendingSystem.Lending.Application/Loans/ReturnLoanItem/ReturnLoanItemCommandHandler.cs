using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Loans;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

internal sealed class ReturnLoanItemCommandHandler(ILoanRepository loans) : IRequestHandler<ReturnLoanItemCommand, Result<ReturnLoanItemResult>>
{
    public async Task<Result<ReturnLoanItemResult>> Handle(ReturnLoanItemCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderId <= 0 || request.ObjectId <= 0)
        {
            return Result<ReturnLoanItemResult>.Failure(LoanErrors.MissingReturnFields());
        }

        var loan = await loans.ReturnItemAsync(request.OrderId, request.ObjectId, cancellationToken);
        return loan.IsSuccess
            ? Result<ReturnLoanItemResult>.Success(Map(loan.Data!))
            : Result<ReturnLoanItemResult>.Failure(loan.Error);
    }

    private static ReturnLoanItemResult Map(UserLoan loan) => new(
        loan.OrderId,
        loan.UserId,
        loan.OrderStartDate,
        loan.OrderEndDate,
        loan.OrderStatus,
        loan.Items.Select(x => new ReturnLoanItemDetailResult(
            x.ObjectDetailId,
            x.ObjectId,
            x.ObjectName,
            x.DetailStatus,
            x.ActualReturnDate)).ToArray());
}
