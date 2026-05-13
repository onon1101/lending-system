using LendingSystem.Domain.Items;
using LendingSystem.Domain.Loans;
using LendingSystem.Domain.Media;
using LendingSystem.Domain.Users;
using LendingSystem.Application.Common;

namespace LendingSystem.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<UserProfile> CreateAsync(string name, string email, string passwordHash, CancellationToken cancellationToken);
    Task<UserProfile?> GetByIdAsync(int userId, CancellationToken cancellationToken);
    Task<UserProfile?> SearchByNameAsync(string username, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int userId, CancellationToken cancellationToken);
}

public interface IItemRepository
{
    Task<Item> CreateAsync(int userId, string objectName, string maker, string material, string description, string imageUrl, CancellationToken cancellationToken);
    Task<Item?> GetByIdAsync(int itemId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemSummary>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemSummary>> GetItemsByUserId(int userId,
        CancellationToken cancellationToken);
    Task<Item?> UpdateAsync(int itemId, string? objectName, string? maker, string? material, string? description, string? currentStatus, string? imageUrl, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemMediaSummary>> GetMediaByItemIdAsync(int itemId, CancellationToken cancellationToken);
}

public interface ILoanRepository
{
    Task<IReadOnlyCollection<UserLoan>> GetActiveLoansByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<Result<UserLoan>> CreateAsync(int? borrowerId, string? borrowerName, IReadOnlyCollection<int> itemIds, int durationHours, CancellationToken cancellationToken);
    Task<Result<UserLoan>> CreateRecordAsync(int ownerId, int? borrowerId, string? borrowerName, int itemId, DateTimeOffset startTime, DateTimeOffset endTime, CancellationToken cancellationToken);
    Task<Result<UserLoan>> UpdateRecordTimeAsync(int ownerId, int orderId, DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteRecordAsync(int ownerId, int orderId, CancellationToken cancellationToken);
    Task<Result<UserLoan>> ReturnItemAsync(int orderId, int objectId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LoanRecord>> GetHistoryByItemIdAsync(int itemId, CancellationToken cancellationToken);
}

public interface IMediaRepository
{
    Task<MediaAsset> CreateAsync(int? orderId, int objectId, string type, string url, string link, string description, CancellationToken cancellationToken);
}
