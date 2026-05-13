using LendingSystem.Application.Abstractions;
using LendingSystem.Application.Common;
using LendingSystem.Domain.Items;
using LendingSystem.Domain.Loans;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.Infrastructure.Persistence;

public sealed class LoanRepository(LendingDbContext db) : ILoanRepository
{
    public async Task<IReadOnlyCollection<UserLoan>> GetActiveLoansByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        var orders = await db.Orders
            .AsNoTracking()
            .Include(x => x.Details)
            .ThenInclude(x => x.Item)
            .Where(x => x.BorrowerId == userId && x.Status == LoanStatuses.OnLoan)
            .OrderBy(x => x.OrderId)
            .ToArrayAsync(cancellationToken);

        return orders.Select(Map).ToArray();
    }

    public async Task<Result<UserLoan>> CreateAsync(int? borrowerId, string? borrowerName, IReadOnlyCollection<int> itemIds, int durationHours, CancellationToken cancellationToken)
    {
        if (itemIds.Count != itemIds.Distinct().Count())
        {
            return Result<UserLoan>.Failure(ErrorCodes.Conflict, "借閱物品不可重複，交易取消");
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var endTime = now.AddHours(durationHours);

        UserEntity? borrower = null;
        if (borrowerId is not null)
        {
            borrower = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == borrowerId && !x.IsDeleted, cancellationToken);
            if (borrower is null)
            {
                await tx.RollbackAsync(cancellationToken);
                return Result<UserLoan>.Failure(ErrorCodes.NotFound, $"使用者 ID {borrowerId} 不存在，交易取消");
            }
        }

        var displayName = borrower?.DisplayName ?? borrowerName?.Trim() ?? "";

        var items = await db.Items
            .Where(x => itemIds.Contains(x.ItemId))
            .ToListAsync(cancellationToken);

        foreach (var objectId in itemIds)
        {
            var item = items.FirstOrDefault(x => x.ItemId == objectId);
            if (item is null || item.CurrentStatus != ItemStatuses.Available)
            {
                await tx.RollbackAsync(cancellationToken);
                return Result<UserLoan>.Failure(ErrorCodes.Conflict, $"物品 ID {objectId} 不可用或不存在，交易取消");
            }
        }

        var order = new OrderEntity
        {
            BorrowerId = borrowerId,
            BorrowerName = displayName,
            StartTime = now,
            EndTime = endTime,
            Status = LoanStatuses.OnLoan
        };

        foreach (var item in items)
        {
            order.Details.Add(new OrderDetailEntity
            {
                ObjectId = item.ItemId,
                DetailStatus = LoanStatuses.OnLoan
            });

            item.CurrentStatus = ItemStatuses.OnLoan;
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return Result<UserLoan>.Success(new UserLoan(order.OrderId, borrowerId ?? 0, now, endTime, LoanStatuses.OnLoan, []));
    }

    public async Task<Result<UserLoan>> CreateRecordAsync(int ownerId, int? borrowerId, string? borrowerName, int itemId, DateTimeOffset startTime, DateTimeOffset endTime, CancellationToken cancellationToken)
    {
        var item = await db.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ItemId == itemId, cancellationToken);
        if (item is null)
        {
            return Result<UserLoan>.Failure(ErrorCodes.NotFound, $"物品 ID {itemId} 不存在");
        }

        if (item.OwnerId != ownerId)
        {
            return Result<UserLoan>.Failure(ErrorCodes.Conflict, $"物品 ID {itemId} 不屬於使用者 ID {ownerId}");
        }

        UserEntity? borrower = null;
        if (borrowerId is not null)
        {
            borrower = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == borrowerId && !x.IsDeleted, cancellationToken);
            if (borrower is null)
            {
                return Result<UserLoan>.Failure(ErrorCodes.NotFound, $"使用者 ID {borrowerId} 不存在");
            }
        }

        var displayName = borrower?.DisplayName ?? borrowerName?.Trim() ?? "";
        var record = new OrderEntity
        {
            BorrowerId = borrowerId,
            BorrowerName = displayName,
            StartTime = startTime,
            EndTime = endTime,
            Status = LoanStatuses.Returned,
        };
        record.Details.Add(new OrderDetailEntity
        {
            ObjectId = itemId,
            DetailStatus = LoanStatuses.Returned,
            ActualReturnTime = endTime
        });

        db.Orders.Add(record);
        await db.SaveChangesAsync(cancellationToken);

        var created = await GetByOrderIdAsync(record.OrderId, cancellationToken);
        return created is null
            ? Result<UserLoan>.Failure(ErrorCodes.NotFound, "Loan record not found")
            : Result<UserLoan>.Success(created);
    }

    public async Task<Result<bool>> DeleteRecordAsync(int ownerId, int orderId, CancellationToken cancellationToken)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var order = await db.Orders
            .Include(x => x.Details)
            .ThenInclude(x => x.Item)
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
        if (order is null)
        {
            await tx.RollbackAsync(cancellationToken);
            return Result<bool>.Failure(ErrorCodes.NotFound, $"借閱紀錄 ID {orderId} 不存在");
        }

        if (order.Details.Count == 0)
        {
            await tx.RollbackAsync(cancellationToken);
            return Result<bool>.Failure(ErrorCodes.Conflict, $"借閱紀錄 ID {orderId} 沒有物品明細");
        }

        if (order.Details.Any(x => x.Item is null || x.Item.OwnerId != ownerId))
        {
            await tx.RollbackAsync(cancellationToken);
            return Result<bool>.Failure(ErrorCodes.Conflict, $"借閱紀錄 ID {orderId} 包含不屬於使用者 ID {ownerId} 的物品");
        }

        foreach (var detail in order.Details.Where(x => x.DetailStatus == LoanStatuses.OnLoan))
        {
            detail.Item!.CurrentStatus = ItemStatuses.Available;
        }

        db.Orders.Remove(order);
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<UserLoan>> UpdateRecordTimeAsync(int ownerId, int orderId, DateTimeOffset? startTime, DateTimeOffset? endTime, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .Include(x => x.Details)
            .ThenInclude(x => x.Item)
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
        if (order is null)
        {
            return Result<UserLoan>.Failure(ErrorCodes.NotFound, $"借閱紀錄 ID {orderId} 不存在");
        }

        if (order.Details.Count == 0)
        {
            return Result<UserLoan>.Failure(ErrorCodes.Conflict, $"借閱紀錄 ID {orderId} 沒有物品明細");
        }

        if (order.Details.Any(x => x.Item is null || x.Item.OwnerId != ownerId))
        {
            return Result<UserLoan>.Failure(ErrorCodes.Conflict, $"借閱紀錄 ID {orderId} 包含不屬於使用者 ID {ownerId} 的物品");
        }

        var updatedStartTime = startTime ?? order.StartTime;
        var updatedEndTime = endTime ?? order.EndTime;
        if (updatedStartTime >= updatedEndTime)
        {
            return Result<UserLoan>.Failure(ErrorCodes.Validation, "start_time must be earlier than end_time");
        }

        var oldEndTime = order.EndTime;
        order.StartTime = updatedStartTime;
        order.EndTime = updatedEndTime;

        if (endTime is not null)
        {
            foreach (var detail in order.Details.Where(x => x.ActualReturnTime == oldEndTime))
            {
                detail.ActualReturnTime = updatedEndTime;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var updated = await GetByOrderIdAsync(orderId, cancellationToken);
        return updated is null
            ? Result<UserLoan>.Failure(ErrorCodes.NotFound, "Loan record not found")
            : Result<UserLoan>.Success(updated);
    }

    public async Task<Result<UserLoan>> ReturnItemAsync(int orderId, int objectId, CancellationToken cancellationToken)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var detail = await db.OrderDetails
            .FirstOrDefaultAsync(
                x => x.OrderId == orderId
                    && x.ObjectId == objectId
                    && x.DetailStatus == LoanStatuses.OnLoan,
                cancellationToken);

        if (detail is null)
        {
            await tx.RollbackAsync(cancellationToken);
            return Result<UserLoan>.Failure(ErrorCodes.Conflict, $"借閱單 {orderId} 中的物品 ID {objectId} 不存在或已歸還");
        }

        var now = DateTimeOffset.UtcNow;
        detail.DetailStatus = LoanStatuses.Returned;
        detail.ActualReturnTime = now;

        var item = await db.Items.FirstOrDefaultAsync(x => x.ItemId == objectId, cancellationToken);
        if (item is not null)
        {
            item.CurrentStatus = ItemStatuses.Available;
        }

        var order = await db.Orders
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
        if (order is not null && order.Details.All(x => x.DetailStatus != LoanStatuses.OnLoan))
        {
            order.Status = LoanStatuses.Returned;
        }

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var loan = await GetByOrderIdAsync(orderId, cancellationToken);
        return loan is null
            ? Result<UserLoan>.Failure(ErrorCodes.NotFound, "Loan not found")
            : Result<UserLoan>.Success(loan);
    }

    public async Task<IReadOnlyCollection<LoanRecord>> GetHistoryByItemIdAsync(int itemId, CancellationToken cancellationToken)
    {
        var itemExists = await db.Items
            .AsNoTracking()
            .AnyAsync(x => x.ItemId == itemId, cancellationToken);
        if (!itemExists)
        {
            return [];
        }

        var records = await db.OrderDetails
            .AsNoTracking()
            .Where(x => x.ObjectId == itemId)
            .OrderByDescending(x => x.Order!.StartTime)
            .Select(x => new LoanRecord(
                x.OrderId,
                x.Order!.StartTime,
                x.Order.EndTime,
                x.Order.User == null ? x.Order.BorrowerName : x.Order.User.DisplayName,
                x.Order.Status))
            .ToArrayAsync(cancellationToken);

        return records.Length == 0 ? [new LoanRecord(null, null, null, null, null)] : records;
    }

    private async Task<UserLoan?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(x => x.Details)
            .ThenInclude(x => x.Item)
            .Where(x => x.OrderId == orderId)
            .FirstOrDefaultAsync(cancellationToken);

        return order is null ? null : Map(order);
    }

    private static UserLoan Map(OrderEntity order) => new(
        order.OrderId,
        order.BorrowerId ?? 0,
        order.StartTime,
        order.EndTime,
        order.Status,
        order.Details
            .OrderBy(x => x.ObjectId)
            .Select(x => new LoanItemDetail(
                x.ObjectDetailId,
                x.ObjectId,
                x.Item?.ObjectName ?? "",
                x.DetailStatus,
                x.ActualReturnTime))
            .ToArray());
}
