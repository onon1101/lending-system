using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

internal sealed class DeleteLoanRecordCommandHandler(ILoanRepository loans) : IRequestHandler<DeleteLoanRecordCommand, Result<DeleteLoanRecordResult>>
{
    public async Task<Result<DeleteLoanRecordResult>> Handle(DeleteLoanRecordCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId <= 0 || request.OrderId <= 0)
        {
            return Result<DeleteLoanRecordResult>.Failure(LoanErrors.MissingDeleteRecordFields());
        }

        var deleted = await loans.DeleteRecordAsync(request.UserId, request.OrderId, cancellationToken);
        return deleted.IsSuccess
            ? Result<DeleteLoanRecordResult>.Success(new DeleteLoanRecordResult(true, $"Delete borrowing record from order_id {request.OrderId} is successful."))
            : Result<DeleteLoanRecordResult>.Failure(deleted.Error);
    }
}
