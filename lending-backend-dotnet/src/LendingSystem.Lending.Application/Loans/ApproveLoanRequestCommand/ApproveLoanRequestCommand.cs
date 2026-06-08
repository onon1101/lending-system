using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Loans.ApproveLoanRequestCommand;

public sealed record ApproveLoanRequestCommand(string BorrowingKey) : ICommand<ApproveLoanRequestResult>;
