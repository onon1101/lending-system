using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

public sealed record ReturnLoanItemCommand(string BorrowingKey) : ICommand<ReturnLoanItemResult>;
