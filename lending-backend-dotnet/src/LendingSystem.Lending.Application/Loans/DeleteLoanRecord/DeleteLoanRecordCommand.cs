using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

public sealed record DeleteLoanRecordCommand(int UserId, int OrderId) : ICommand<DeleteLoanRecordResult>;
