using LendingSystem.Lending.Domain.Aggregate.Item;
using LendingSystem.Lending.Domain.Aggregate.Loans;
using LendingSystem.Lending.Application.Media;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Lending.Application.Abstractions;

public interface IItemCommandRepository
{
    Task<Item> CreateAsync(int userId, string objectName, string maker, string material, string description, string imageUrl, CancellationToken cancellationToken);
    Task<Item?> GetByIdForCommandAsync(int itemId, CancellationToken cancellationToken);
    Task<Item?> UpdateAsync(int itemId, string? objectName, string? maker, string? material, string? description, string? currentStatus, string? imageUrl, CancellationToken cancellationToken);
}

public interface IItemQueryRepository
{
    Task<Item?> GetByIdAsync(int itemId, CancellationToken cancellationToken);
    Task<Item?> GetByNameAsync(int userId, string itemName, CancellationToken cancellation);
    Task<Item?> GetByNameAsync(string ownerUsername, string itemName, CancellationToken cancellation);
    Task<IReadOnlyCollection<ItemSummary>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemSummary>?> GetItemsByUserId(int userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemSummary>?> GetItemsByUserName(string username, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemMediaSummary>> GetMediaByItemIdAsync(int itemId, CancellationToken cancellationToken);
    Task<int?> GetUserIdByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<bool> GetItemExistsAsync(string username, string itemName, CancellationToken cancellationToken);
}

public interface ILoanCommandRepository
{
    Task<Result<UserLoan>> CreateAsync(int? borrowerId, string? borrowerName, IReadOnlyCollection<int> itemIds, int durationDays, CancellationToken cancellationToken);
    Task<Result<UserLoan>> CreateRequestAsync(int borrowerId, string itemOwnerUsername, string itemName, DateOnly startDate, int durationDays, CancellationToken cancellationToken);
    Task<Result<UserLoan>> CreateRecordAsync(int ownerId, int? borrowerId, string? borrowerName, int itemId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);
    Task<Result<UserLoan>> UpdateRecordTimeAsync(int ownerId, int orderId, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteRecordAsync(int ownerId, int orderId, CancellationToken cancellationToken);
    Task<Result<UserLoan>> ReturnItemAsync(int orderId, int objectId, CancellationToken cancellationToken);
    Task<Result<UserLoan>> ReturnItemAsync(int orderId, CancellationToken cancellationToken);
}

public interface ILoanQueryRepository
{
    Task<IReadOnlyCollection<UserLoan>> GetActiveLoansByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LoanRecord>> GetHistoryByItemIdAsync(int itemId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LoanRequestRecord>> GetRequestsByOwnerIdAsync(int ownerId, CancellationToken cancellationToken);
}

public interface IMediaCommandRepository
{
    Task<MediaAsset> CreateItemMediaAsync(int itemId, string type, string url, string link, string description, CancellationToken cancellationToken);
    Task<MediaAsset> CreateLendingMediaAsync(int orderId, int itemId, string type, string url, string link, string description, CancellationToken cancellationToken);
}
