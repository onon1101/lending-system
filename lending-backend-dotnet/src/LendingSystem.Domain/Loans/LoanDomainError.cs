using LendingSystem.Domain.Commons;

namespace LendingSystem.Application.Loans;

public static class LoanDomainError
{
    /// <summary>
    /// 開始時間不得大於結束時間
    /// </summary>
    /// <returns></returns>
    public static DomainErrors StartDateMustBeEarlierThanEndDate() =>
        new("LOAN_START_DATE_MUST_BE_EARLIER_THAN_END_DATE",
            "start_date must be earlier than end_date",
            "Loan start date must be earlier than end date");

    /// <summary>
    /// 查無借閱紀錄
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    public static DomainErrors LoanRecordNotFound(int orderId) =>
        new("LOAN_ISSUE",
                $"Loan record {orderId} was not found.",
                "Loan record was not found.");

    /// <summary>
    /// 物品不屬於使用者
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public static DomainErrors ItemDoesNotBelongToOwner(int itemId, int ownerId) =>
        new("ITEM_ISSUE",
            $"Item {itemId} doesn't belong to owner {ownerId}",
            "Some item issue");
}