using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Loans.RejectLoanRequestCommand;

public sealed record RejectLoanRequestCommand(string BorrowingKey) : ICommand<RejectLoanRequestResult>;
