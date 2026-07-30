using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Application.Loans;

public static class LoanErrors
{
    public static Errors ValidateFieldError() =>
    new("Loan.ValidationFieldError", "Some fields are missing or not satisfied for rule", ErrorType.Validation);
    public static Errors MissingItemOrBorrowerNotFound(string username, string itemName) =>
    new("Loan.ItemOrBorrowerNotFound", "The Borrower or item does not exist", ErrorType.NotFound);
    public static Errors MissingCreateFields() =>
        new("Loan.MissingCreateFields", "Missing required fields", ErrorType.Validation);

    public static Errors MissingReturnFields() =>
        new("Loan.MissingReturnFields", "Missing required fields", ErrorType.Validation);

    public static Errors MissingCreateRecordFields() =>
        new("Loan.MissingCreateRecordFields", "Missing required fields", ErrorType.Validation);

    public static Errors MissingDeleteRecordFields() =>
        new("Loan.MissingDeleteRecordFields", "Missing required fields", ErrorType.Validation);

    public static Errors MissingUpdateRecordTimeFields() =>
        new("Loan.MissingUpdateRecordTimeFields", "Missing required fields", ErrorType.Validation);

    public static Errors MissingLoanRequestDecisionFields() =>
        new("Loan.MissingRequestDecisionFields", "Missing required fields", ErrorType.Validation);

    public static Errors AccessOwnBorrowingsOnly() =>
        new("Loan.AccessOwnBorrowingsOnly", "You can only access your own borrowings", ErrorType.Unauthorized);

    public static Errors CreateBorrowingsForSelfOnly() =>
        new("Loan.CreateBorrowingsForSelfOnly", "You can only create borrowings for yourself", ErrorType.Unauthorized);

    public static Errors ManageOwnItemRecordsOnly() =>
        new("Loan.ManageOwnItemRecordsOnly", "You can only manage your own item records", ErrorType.Unauthorized);

    public static Errors ItemNotFound() =>
        new("Loan.ItemNotFound", "Item not found", ErrorType.NotFound);

    public static Errors BorrowerNotFound(long borrowerId) =>
        new("Loan.BorrowerNotFound", "Borrower was not found", ErrorType.NotFound);

    public static Errors ItemOwnerOrItemNotFound(string itemOwnerUsername, string itemName) =>
        new("Loan.ItemOwnerOrItemNotFound", "Item owner or item was not found", ErrorType.NotFound);

    public static Errors LoanRequestNotFound() =>
        new("Loan.RequestNotFound", "Loan request was not found", ErrorType.NotFound);
}
