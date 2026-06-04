using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Application.Loans;

public static class LoanErrors
{
    public static ApplicationErrors ValidateFieldError() =>
    new ("VALIDATION_FIELD_ERROR",
        "Some fields are missing or not satisfied for rule",
        "Some fields are missing or not satisfied for rule",
        ErrorType.Validation);
    public static ApplicationErrors MissingItemOrBorrowerNotFound(string username, string itemName) =>
    new ("ITEM_OR_BORROWER_NOT_FOUND",
        $"The Borrower: {username} or item: {itemName} does not exist",
        "The Borrower or item does not exist",
        ErrorType.NotFound);
    public static ApplicationErrors MissingCreateFields() =>
        new(
            "LOAN_MISSING_CREATE_FIELDS",
            "Missing required fields (borrower_username or borrower_name, items, duration_days)",
            "Missing required fields",
            ErrorType.Validation);

    public static ApplicationErrors MissingReturnFields() =>
        new(
            "LOAN_MISSING_RETURN_FIELDS",
            "Missing required fields (borrowing_key)",
            "Missing required fields",
            ErrorType.Validation);

    public static ApplicationErrors MissingCreateRecordFields() =>
        new(
            "LOAN_MISSING_CREATE_RECORD_FIELDS",
            "Missing required fields (owner_username, borrower_username or borrower_name, object_name, start_date, end_date)",
            "Missing required fields",
            ErrorType.Validation);

    public static ApplicationErrors MissingDeleteRecordFields() =>
        new(
            "LOAN_MISSING_DELETE_RECORD_FIELDS",
            "Missing required fields (owner_username, borrowing_key)",
            "Missing required fields",
            ErrorType.Validation);

    public static ApplicationErrors MissingUpdateRecordTimeFields() =>
        new(
            "LOAN_MISSING_UPDATE_RECORD_TIME_FIELDS",
            "Missing required fields (owner_username, borrowing_key, start_date or end_date)",
            "Missing required fields",
            ErrorType.Validation);

    public static ApplicationErrors AccessOwnBorrowingsOnly() =>
        new(
            "ACCESS_OWN_BORROWINGS_ONLY",
            "You can only access your own borrowings",
            "You can only access your own borrowings",
            ErrorType.Unauthorized);

    public static ApplicationErrors CreateBorrowingsForSelfOnly() =>
        new(
            "CREATE_BORROWINGS_FOR_SELF_ONLY",
            "You can only create borrowings for yourself",
            "You can only create borrowings for yourself",
            ErrorType.Unauthorized);

    public static ApplicationErrors ManageOwnItemRecordsOnly() =>
        new(
            "MANAGE_OWN_ITEM_RECORDS_ONLY",
            "You can only manage your own item records",
            "You can only manage your own item records",
            ErrorType.Unauthorized);

    public static ApplicationErrors ItemNotFound() =>
        new("ITEM_NOT_FOUND",
            "Item not found",
            "Item not found",
            ErrorType.NotFound);

    public static ApplicationErrors BorrowerNotFound(long borrowerId) =>
        new(
            "BORROWER_NOT_FOUND",
            $"Borrower {borrowerId} was not found",
            "Borrower was not found",
            ErrorType.NotFound);

    public static ApplicationErrors ItemOwnerOrItemNotFound(string itemOwnerUsername, string itemName) =>
        new(
            "ITEM_OWNER_OR_ITEM_NOT_FOUND",
            $"Item owner {itemOwnerUsername} or item {itemName} was not found",
            "Item owner or item was not found",
            ErrorType.NotFound);
}
