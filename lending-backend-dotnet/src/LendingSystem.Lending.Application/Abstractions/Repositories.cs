using LendingSystem.Lending.Domain.Items;
using LendingSystem.Lending.Domain.Loans;
using LendingSystem.Lending.Domain.Media;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Lending.Application.Abstractions;

public interface IItemRepository
{
    Task<Item> CreateAsync(int userId, string objectName, string maker, string material, string description, string imageUrl, CancellationToken cancellationToken);
    Task<Item?> GetByIdAsync(int itemId, CancellationToken cancellationToken);
    Task<Item?> GetByNameAsync(int userId, string itemName, CancellationToken cancellation);
    Task<IReadOnlyCollection<ItemSummary>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemSummary>?> GetItemsByUserId(int userId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemSummary>?> GetItemsByUserName(string username, CancellationToken cancellationToken);
    Task<Item?> UpdateAsync(int itemId, string? objectName, string? maker, string? material, string? description, string? currentStatus, string? imageUrl, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemMediaSummary>> GetMediaByItemIdAsync(int itemId, CancellationToken cancellationToken);
}

public interface ILoanRepository
{
    Task<IReadOnlyCollection<UserLoan>> GetActiveLoansByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<Result<UserLoan>> CreateAsync(int? borrowerId, string? borrowerName, IReadOnlyCollection<int> itemIds, int durationDays, CancellationToken cancellationToken);
    Task<Result<UserLoan>> CreateRecordAsync(int ownerId, int? borrowerId, string? borrowerName, int itemId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken);
    Task<Result<UserLoan>> UpdateRecordTimeAsync(int ownerId, int orderId, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteRecordAsync(int ownerId, int orderId, CancellationToken cancellationToken);
    Task<Result<UserLoan>> ReturnItemAsync(int orderId, int objectId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LoanRecord>> GetHistoryByItemIdAsync(int itemId, CancellationToken cancellationToken);
}

public interface IMediaRepository
{
    Task<MediaAsset> CreateItemMediaAsync(int itemId, string type, string url, string link, string description, CancellationToken cancellationToken);
    Task<MediaAsset> CreateLendingMediaAsync(int orderId, int itemId, string type, string url, string link, string description, CancellationToken cancellationToken);
}
