using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Loans.ReturnLoanItem;

public sealed record ReturnLoanItemCommand(string BorrowingKey) : ICommand<ReturnLoanItemResult>;
