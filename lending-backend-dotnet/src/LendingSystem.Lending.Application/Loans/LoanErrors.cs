using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Application.Loans;

public static class LoanErrors
{
    public static ApplicationErrors MissingCreateFields() =>
        new(
            "LOAN_MISSING_CREATE_FIELDS",
            "Missing required fields (borrower_id or borrower_name, items_id, duration_days)",
            "Missing required fields",
            ErrorType.Validation);

    public static ApplicationErrors MissingReturnFields() =>
        new(
            "LOAN_MISSING_RETURN_FIELDS",
            "Missing required fields (order_id, object_id)",
            "Missing required fields",
            ErrorType.Validation);

    public static ApplicationErrors MissingCreateRecordFields() =>
        new(
            "LOAN_MISSING_CREATE_RECORD_FIELDS",
            "Missing required fields (user_id, borrower_id or borrower_name, item_id, start_date, end_date)",
            "Missing required fields",
            ErrorType.Validation);

    public static ApplicationErrors MissingDeleteRecordFields() =>
        new(
            "LOAN_MISSING_DELETE_RECORD_FIELDS",
            "Missing required fields (user_id, order_id)",
            "Missing required fields",
            ErrorType.Validation);

    public static ApplicationErrors MissingUpdateRecordTimeFields() =>
        new(
            "LOAN_MISSING_UPDATE_RECORD_TIME_FIELDS",
            "Missing required fields (user_id, order_id, start_date or end_date)",
            "Missing required fields",
            ErrorType.Validation);
}
