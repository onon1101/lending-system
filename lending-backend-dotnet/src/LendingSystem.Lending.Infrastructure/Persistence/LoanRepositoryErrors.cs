using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Infrastructure.Persistence;

public static class LoanRepositoryErrors
{
    public static RepositoryErrors DuplicateBorrowingItems() =>
        new("LOAN_DUPLICATE_BORROWING_ITEMS", "借閱物品不可重複，交易取消", "Borrowing items cannot be duplicated", ErrorType.Conflict);

    public static RepositoryErrors ItemUnavailableOrNotFound(int itemId) =>
        new("LOAN_ITEM_UNAVAILABLE_OR_NOT_FOUND", $"物品 ID {itemId} 不可用或不存在，交易取消", "Some item is unavailable or does not exist", ErrorType.Conflict);

    public static RepositoryErrors LoanNotFound() =>
        new("LOAN_NOT_FOUND", "Loan not found", "Loan not found", ErrorType.NotFound);

    public static RepositoryErrors ItemNotFound(int itemId) =>
        new("LOAN_ITEM_NOT_FOUND", $"物品 ID {itemId} 不存在", "Item not found", ErrorType.NotFound);

    public static RepositoryErrors ItemDoesNotBelongToOwner(int itemId, int ownerId) =>
        new("LOAN_ITEM_DOES_NOT_BELONG_TO_OWNER", $"物品 ID {itemId} 不屬於使用者 ID {ownerId}", "Item does not belong to owner", ErrorType.Conflict);

    public static RepositoryErrors LoanRecordNotFound() =>
        new("LOAN_RECORD_NOT_FOUND", "Loan record not found", "Loan record not found", ErrorType.NotFound);

    public static RepositoryErrors LoanRecordNotFound(int orderId) =>
        new("LOAN_RECORD_NOT_FOUND", $"借閱紀錄 ID {orderId} 不存在", "Loan record not found", ErrorType.NotFound);

    public static RepositoryErrors LoanRecordDoesNotBelongToOwner(int orderId, int ownerId) =>
        new("LOAN_RECORD_DOES_NOT_BELONG_TO_OWNER", $"借閱紀錄 ID {orderId} 包含不屬於使用者 ID {ownerId} 的物品", "Loan record does not belong to owner", ErrorType.Conflict);

    public static RepositoryErrors LoanItemAlreadyReturnedOrNotFound(int orderId, int itemId) =>
        new("LOAN_ITEM_ALREADY_RETURNED_OR_NOT_FOUND", $"借閱單 {orderId} 中的物品 ID {itemId} 不存在或已歸還", "Loan item does not exist or has already been returned", ErrorType.Conflict);

    public static RepositoryErrors BorrowerNotFound(int borrowerId) =>
        new("LOAN_BORROWER_NOT_FOUND", $"使用者 ID {borrowerId} 不存在", "Borrower not found", ErrorType.NotFound);

    public static RepositoryErrors ItemOwnerOrItemNotFound(string ownerUsername, string itemName) =>
        new("LOAN_ITEM_OWNER_OR_ITEM_NOT_FOUND", $"使用者 {ownerUsername} 或物品 {itemName} 不存在", "Item owner or item not found", ErrorType.NotFound);
}
