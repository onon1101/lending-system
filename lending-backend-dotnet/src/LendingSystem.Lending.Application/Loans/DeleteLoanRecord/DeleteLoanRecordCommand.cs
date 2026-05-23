using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

public sealed record DeleteLoanRecordCommand(string OwnerUsername, string BorrowingKey) : ICommand<DeleteLoanRecordResult>;
