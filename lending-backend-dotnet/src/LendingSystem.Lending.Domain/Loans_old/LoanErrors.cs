using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Loans;

public static class LoanErrors
{
    public static Errors OnlyRequestedLoanCanBeApproved() =>
        new("Loan.OnlyRequestedLoanCanBeApproved", "只能同意待處理的借閱請求");

    public static Errors OnlyRequestedLoanCanBeRejected() =>
        new("Loan.OnlyRequestedLoanCanBeRejected", "只能拒絕待處理的借閱請求");

    public static Errors StartDateMustBeEarlierThanEndDate() =>
        new("Loan.StartDateMustBeEarlierThanEndDate", "Loan start date must be earlier than end date");

    public static Errors DurationDaysMustBePositive() =>
        new("Loan.DurationDaysMustBePositive", "Loan duration days must be greater than 0");

    public static Errors CannotBorrowOwnItem() =>
        new("Loan.CannotBorrowOwnItem", "Borrower cannot borrow their own item");

    public static Errors ItemMustBeAvailable(long itemId) =>
        new("Loan.ItemMustBeAvailable", $"Item {itemId} must be available before creating a loan request");

    public static Errors LoanRecordNotFound(long orderId) =>
        new("Loan.RecordNotFound", $"Loan record {orderId} was not found", ErrorType.NotFound);

    public static Errors ItemDoesNotBelongToOwner(long itemId, long ownerId) =>
        new(
            "Loan.ItemDoesNotBelongToOwner",
            $"Item {itemId} doesn't belong to owner {ownerId}",
            ErrorType.Conflict);
}
