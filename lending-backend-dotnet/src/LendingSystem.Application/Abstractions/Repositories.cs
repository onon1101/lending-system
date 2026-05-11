using LendingSystem.Domain.Items;
using LendingSystem.Domain.Loans;
using LendingSystem.Domain.Media;
using LendingSystem.Domain.Users;

namespace LendingSystem.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<UserProfile> CreateAsync(string name, string email, string passwordHash, CancellationToken cancellationToken);
    Task<UserProfile?> GetByIdAsync(int userId, CancellationToken cancellationToken);
    Task<UserProfile?> SearchByNameAsync(string username, CancellationToken cancellationToken);
}

public interface IItemRepository
{
    Task<Item> CreateAsync(string objectName, string description, CancellationToken cancellationToken);
    Task<Item?> GetByIdAsync(int objectId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemSummary>> GetAllAsync(CancellationToken cancellationToken);
    Task<Item?> UpdateAsync(int objectId, string? objectName, string? description, string? currentStatus, string? imageUrl, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ItemMediaSummary>> GetMediaByItemIdAsync(int itemId, CancellationToken cancellationToken);
}

public interface ILoanRepository
{
    Task<IReadOnlyCollection<UserLoan>> GetActiveLoansByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<UserLoan> CreateAsync(int userId, IReadOnlyCollection<int> itemIds, int durationHours, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<LoanRecord>> GetHistoryByItemIdAsync(int itemId, CancellationToken cancellationToken);
}

public interface IMediaRepository
{
    Task<MediaAsset> CreateAsync(int? orderId, int objectId, string type, string url, string link, string description, CancellationToken cancellationToken);
}
