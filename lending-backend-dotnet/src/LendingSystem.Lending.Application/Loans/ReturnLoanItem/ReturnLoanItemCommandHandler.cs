using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Loans;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans.ReturnLoanItem;

internal sealed class ReturnLoanItemCommandHandler(ILoanCommandRepository loans) : IRequestHandler<ReturnLoanItemCommand, Result<ReturnLoanItemResult>>
{
    public async Task<Result<ReturnLoanItemResult>> Handle(ReturnLoanItemCommand request, CancellationToken cancellationToken)
    {
        if (!PublicResourceKey.TryGetInt("borrowing", request.BorrowingKey, out var orderId) || orderId <= 0)
        {
            return Result<ReturnLoanItemResult>.Failure(LoanErrors.MissingReturnFields());
        }

        var loan = await loans.ReturnItemAsync(orderId, cancellationToken);
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
