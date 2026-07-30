using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Infrastructure.Persistence;

public static class LoanErrors
{
    public static Errors DuplicateBorrowingItems() =>
        new("Loan.DuplicateBorrowingItems", "Borrowing items cannot be duplicated", ErrorType.Conflict);

    public static Errors ItemUnavailableOrNotFound(long itemId) =>
        new("Loan.ItemUnavailableOrNotFound", "Some item is unavailable or does not exist", ErrorType.Conflict);

    public static Errors LoanNotFound() =>
        new("Loan.NotFound", "Loan not found", ErrorType.NotFound);

    public static Errors ItemNotFound(long itemId) =>
        new("Loan.ItemNotFound", "Item not found", ErrorType.NotFound);

    public static Errors ItemDoesNotBelongToOwner(long itemId, long ownerId) =>
        new("Loan.ItemDoesNotBelongToOwner", "Item does not belong to owner", ErrorType.Conflict);

    public static Errors LoanRecordNotFound() =>
        new("Loan.RecordNotFound", "Loan record not found", ErrorType.NotFound);

    public static Errors LoanRecordNotFound(long orderId) =>
        new("Loan.RecordNotFound", "Loan record not found", ErrorType.NotFound);

    public static Errors LoanRecordDoesNotBelongToOwner(long orderId, long ownerId) =>
        new("Loan.RecordDoesNotBelongToOwner", "Loan record does not belong to owner", ErrorType.Conflict);

    public static Errors LoanItemAlreadyReturnedOrNotFound(long orderId, long itemId) =>
        new("Loan.ItemAlreadyReturnedOrNotFound", "Loan item does not exist or has already been returned", ErrorType.Conflict);

    public static Errors BorrowerNotFound(long borrowerId) =>
        new("Loan.BorrowerNotFound", "Borrower not found", ErrorType.NotFound);

    public static Errors ItemOwnerOrItemNotFound(string ownerUsername, string itemName) =>
        new("Loan.ItemOwnerOrItemNotFound", "Item owner or item not found", ErrorType.NotFound);
}
