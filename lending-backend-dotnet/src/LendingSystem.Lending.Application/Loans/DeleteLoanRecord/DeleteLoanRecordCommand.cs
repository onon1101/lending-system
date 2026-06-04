using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Loans.DeleteLoanRecord;

public sealed record DeleteLoanRecordCommand(string OwnerUsername, string BorrowingKey) : ICommand<DeleteLoanRecordResult>;
