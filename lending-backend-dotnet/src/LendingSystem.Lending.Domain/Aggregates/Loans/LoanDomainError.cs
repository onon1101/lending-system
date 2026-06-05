using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Aggregates.Loans;

public static class LoanDomainError
{
    /// <summary>
    /// 開始時間不得大於結束時間
    /// </summary>
    public static DomainErrors StartDateMustBeEarlierThanEndDate() =>
        new("LOAN_START_DATE_MUST_BE_EARLIER_THAN_END_DATE",
            "start_date must be earlier than end_date",
            "Loan start date must be earlier than end date");

    public static DomainErrors DurationDaysMustBePositive() =>
        new(
            "LOAN_DURATION_DAYS_MUST_BE_POSITIVE",
            "duration_days must be greater than 0",
            "Loan duration days must be greater than 0");

    public static DomainErrors CannotBorrowOwnItem() =>
        new(
            "LOAN_CANNOT_BORROW_OWN_ITEM",
            "Borrower cannot borrow their own item",
            "Borrower cannot borrow their own item");

    public static DomainErrors ItemMustBeAvailable(long itemId) =>
        new(
            "LOAN_ITEM_MUST_BE_AVAILABLE",
            $"Item {itemId} must be available before creating a loan request",
            "Item must be available before creating a loan request");

    /// <summary>
    /// 查無借閱紀錄
    /// </summary>
    public static DomainErrors LoanRecordNotFound(long orderId) =>
        new("LOAN_ISSUE",
            $"Loan record {orderId} was not found.",
            "Loan record was not found.");

    /// <summary>
    /// 物品不屬於使用者
    /// </summary>
    public static DomainErrors ItemDoesNotBelongToOwner(long itemId, long ownerId) =>
        new("ITEM_ISSUE",
            $"Item {itemId} doesn't belong to owner {ownerId}",
            "Some item issue");
}
