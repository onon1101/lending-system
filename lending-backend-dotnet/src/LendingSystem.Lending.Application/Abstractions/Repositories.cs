using LendingSystem.Lending.Domain.Aggregate.Item;
using LendingSystem.Lending.Domain.Aggregate.Loans;
using LendingSystem.Lending.Application.Media;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Lending.Application.Abstractions;

public interface IItemCommandRepository
{
    Task<Item> CreateAsync(long userId, string objectName, string maker, string material, string description, string imageUrl, CancellationToken cancellationToken);
    Task<Item?> GetByIdForCommandAsync(long itemId, CancellationToken cancellationToken);
    Task<Item?> UpdateAsync(long itemId, string? objectName, string? maker, string? material, string? description, string? currentStatus, string? imageUrl, CancellationToken cancellationToken);
}

public interface IItemQueryRepository
{
    Task<Item?> GetByIdAsync(long itemId, CancellationToken cancellationToken);
    Task<Item?> GetByNameAsync(long userId, string itemName, CancellationToken cancellation);
    Task<Item?> GetByNameAsync(string ownerUsername, string itemName, CancellationToken cancellation);
    Task<IReadOnlyCollection<ItemSummary>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemSummary>?> GetItemsByUserId(long userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemSummary>?> GetItemsByUserName(string username, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemMediaSummary>> GetMediaByItemIdAsync(long itemId, CancellationToken cancellationToken);
    Task<long?> GetUserIdByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<bool> GetItemExistsAsync(string username, string itemName, CancellationToken cancellationToken);
}

public interface ILoanCommandRepository
{
    Task<Result<UserLoan>> CreateAsync(long? borrowerId, string? borrowerName, IReadOnlyCollection<long> itemIds, int durationDays, CancellationToken cancellationToken);
    Task<LoanRequestUser?> GetActiveRequestUserAsync(long userId, CancellationToken cancellationToken);
    Task<LoanRequestItem?> GetRequestItemAsync(string itemOwnerUsername, string itemName, CancellationToken cancellationToken);
    Task<LoanBorrowerDetail> PrepareBorrowerDetailReferenceAsync(long borrowerId, string borrowerName, long ownerId, DateOnly today, CancellationToken cancellationToken);
    Task<Result<UserLoan>> SaveRequestAsync(LoansAggregate aggregate, LoanBorrowerDetail borrowerDetail, DateOnly today, CancellationToken cancellationToken);
    Task<Result<UserLoan>> CreateRecordAsync(long ownerId, long? borrowerId, string? borrowerName, long itemId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);
    Task<Result<UserLoan>> UpdateRecordTimeAsync(long ownerId, long orderId, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteRecordAsync(long ownerId, long orderId, CancellationToken cancellationToken);
    Task<Result<UserLoan>> ReturnItemAsync(long orderId, long objectId, CancellationToken cancellationToken);
    Task<Result<UserLoan>> ReturnItemAsync(long orderId, CancellationToken cancellationToken);
}

public sealed record LoanRequestUser(long UserId, string Name);

public sealed record LoanRequestItem(
    long ItemId,
    string ItemName,
    string CurrentStatus,
    long OwnerId,
    string OwnerName);

public sealed record LoanBorrowerDetail(
    long BorrowerDetailId,
    long BorrowerUserId,
    string BorrowerName,
    long OwnerId,
    bool IsNew);

public interface ILoanQueryRepository
{
    Task<IReadOnlyCollection<UserLoan>> GetActiveLoansByUserIdAsync(long userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LoanRecord>> GetHistoryByItemIdAsync(long itemId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LoanRequestRecord>> GetRequestsByOwnerIdAsync(long ownerId, CancellationToken cancellationToken);
}

public interface IMediaCommandRepository
{
    Task<MediaAsset> CreateItemMediaAsync(long itemId, string type, string url, string link, string description, CancellationToken cancellationToken);
    Task<MediaAsset> CreateLendingMediaAsync(long orderId, long itemId, string type, string url, string link, string description, CancellationToken cancellationToken);
}
