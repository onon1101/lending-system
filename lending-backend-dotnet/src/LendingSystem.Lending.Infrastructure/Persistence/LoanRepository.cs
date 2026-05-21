using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.Lending.Domain.Items;
using LendingSystem.Lending.Domain.Loans;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.Lending.Infrastructure.Persistence;

public sealed class LoanRepository(LendingDbContext db, IQueryConnectionFactory queryConnectionFactory) : ILoanRepository
{
    public async Task<IReadOnlyCollection<UserLoan>> GetActiveLoansByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                o.order_id as OrderId,
                coalesce(bd.user_id, 0) as UserId,
                o.start_date as OrderStartDate,
                o.end_date as OrderEndDate,
                o.status as OrderStatus,
                o.order_id as ObjectDetailId,
                o.item_id as ObjectId,
                coalesce(i.object_name, '') as ObjectName,
                o.status as DetailStatus,
                o.actual_return_date as ActualReturnDate
            from orders o
            join borrower_details bd on bd.borrower_detail_id = o.borrower_detail_id
            left join items i on i.item_id = o.item_id
            where bd.user_id = @UserId
              and o.status = @Status
            order by o.order_id;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<UserLoanRow>(
            new CommandDefinition(sql, new { UserId = userId, Status = LoanStatuses.OnLoan }, cancellationToken: cancellationToken));
        return rows.Select(Map).ToArray();
    }

    public async Task<Result<UserLoan>> CreateAsync(int? borrowerId, string? borrowerName, IReadOnlyCollection<int> itemIds, int durationDays, CancellationToken cancellationToken)
    {
        if (itemIds.Count != itemIds.Distinct().Count())
        {
            return Result<UserLoan>.Failure(LoanRepositoryErrors.DuplicateBorrowingItems());
        }

        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var startDate = Today();
        var endDate = startDate.AddDays(durationDays);

        var borrowerResult = await GetOrCreateBorrowerDetailAsync(borrowerId, borrowerName, null, startDate, cancellationToken);
        if (!borrowerResult.IsSuccess)
        {
            await tx.RollbackAsync(cancellationToken);
            return Result<UserLoan>.Failure(borrowerResult.Error);
        }

        var items = await db.Items
            .Where(x => itemIds.Contains(x.ItemId))
            .ToListAsync(cancellationToken);

        foreach (var objectId in itemIds)
        {
            var item = items.FirstOrDefault(x => x.ItemId == objectId);
            if (item is null || item.CurrentStatus != ItemStatuses.Available)
            {
                await tx.RollbackAsync(cancellationToken);
                return Result<UserLoan>.Failure(LoanRepositoryErrors.ItemUnavailableOrNotFound(objectId));
            }
        }

        var orders = items.Select(item =>
        {
            item.CurrentStatus = ItemStatuses.OnLoan;
            return new OrderEntity
            {
                BorrowerDetailId = borrowerResult.Data!.BorrowerDetailId,
                ObjectId = item.ItemId,
                StartDate = startDate,
                EndDate = endDate,
                Status = LoanStatuses.OnLoan
            };
        }).ToArray();

        db.Orders.AddRange(orders);
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var created = await GetByOrderIdAsync(orders[0].OrderId, cancellationToken);
        return created is null
            ? Result<UserLoan>.Failure(LoanRepositoryErrors.LoanNotFound())
            : Result<UserLoan>.Success(created);
    }

    public async Task<Result<UserLoan>> CreateRecordAsync(int ownerId, int? borrowerId, string? borrowerName, int itemId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        var item = await db.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ItemId == itemId, cancellationToken);
        if (item is null)
        {
            return Result<UserLoan>.Failure(LoanRepositoryErrors.ItemNotFound(itemId));
        }

        if (item.OwnerId != ownerId)
        {
            return Result<UserLoan>.Failure(LoanRepositoryErrors.ItemDoesNotBelongToOwner(itemId, ownerId));
        }

        var borrowerResult = await GetOrCreateBorrowerDetailAsync(borrowerId, borrowerName, ownerId, Today(), cancellationToken);
        if (!borrowerResult.IsSuccess)
        {
            return Result<UserLoan>.Failure(borrowerResult.Error);
        }

        var record = new OrderEntity
        {
            BorrowerDetailId = borrowerResult.Data!.BorrowerDetailId,
            ObjectId = itemId,
            StartDate = startDate,
            EndDate = endDate,
            ActualReturnDate = endDate,
            Status = LoanStatuses.Returned,
        };

        db.Orders.Add(record);
        await db.SaveChangesAsync(cancellationToken);

        var created = await GetByOrderIdAsync(record.OrderId, cancellationToken);
        return created is null
            ? Result<UserLoan>.Failure(LoanRepositoryErrors.LoanRecordNotFound())
            : Result<UserLoan>.Success(created);
    }

    public async Task<Result<bool>> DeleteRecordAsync(int ownerId, int orderId, CancellationToken cancellationToken)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var order = await db.Orders
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
        if (order is null)
        {
            await tx.RollbackAsync(cancellationToken);
            return Result<bool>.Failure(LoanRepositoryErrors.LoanRecordNotFound(orderId));
        }

        if (order.Item is null || order.Item.OwnerId != ownerId)
        {
            await tx.RollbackAsync(cancellationToken);
            return Result<bool>.Failure(LoanRepositoryErrors.LoanRecordDoesNotBelongToOwner(orderId, ownerId));
        }

        if (order.Status == LoanStatuses.OnLoan)
        {
            order.Item.CurrentStatus = ItemStatuses.Available;
        }

        db.Orders.Remove(order);
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<UserLoan>> UpdateRecordTimeAsync(int ownerId, int orderId, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
        if (order is null)
        {
            return Result<UserLoan>.Failure(LoanRepositoryErrors.LoanRecordNotFound(orderId));
        }

        if (order.Item is null || order.Item.OwnerId != ownerId)
        {
            return Result<UserLoan>.Failure(LoanRepositoryErrors.LoanRecordDoesNotBelongToOwner(orderId, ownerId));
        }

        var updatedStartDate = startDate ?? order.StartDate;
        var updatedEndDate = endDate ?? order.EndDate;
        if (updatedStartDate >= updatedEndDate)
        {
            return Result<UserLoan>.Failure(LoanDomainError.StartDateMustBeEarlierThanEndDate());
        }

        var oldEndDate = order.EndDate;
        order.StartDate = updatedStartDate;
        order.EndDate = updatedEndDate;

        if (endDate is not null && order.ActualReturnDate == oldEndDate)
        {
            order.ActualReturnDate = updatedEndDate;
        }

        await db.SaveChangesAsync(cancellationToken);

        var updated = await GetByOrderIdAsync(orderId, cancellationToken);
        return updated is null
            ? Result<UserLoan>.Failure(LoanRepositoryErrors.LoanRecordNotFound())
            : Result<UserLoan>.Success(updated);
    }

    public async Task<Result<UserLoan>> ReturnItemAsync(int orderId, int objectId, CancellationToken cancellationToken)
    {
        await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);

        var order = await db.Orders
            .Include(x => x.Item)
            .FirstOrDefaultAsync(
                x => x.OrderId == orderId
                    && x.ObjectId == objectId
                    && x.Status == LoanStatuses.OnLoan,
                cancellationToken);

        if (order is null)
        {
            await tx.RollbackAsync(cancellationToken);
            return Result<UserLoan>.Failure(LoanRepositoryErrors.LoanItemAlreadyReturnedOrNotFound(orderId, objectId));
        }

        order.Status = LoanStatuses.Returned;
        order.ActualReturnDate = Today();

        if (order.Item is not null)
        {
            order.Item.CurrentStatus = ItemStatuses.Available;
        }

        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var loan = await GetByOrderIdAsync(orderId, cancellationToken);
        return loan is null
            ? Result<UserLoan>.Failure(LoanRepositoryErrors.LoanNotFound())
            : Result<UserLoan>.Success(loan);
    }

    public async Task<IReadOnlyCollection<LoanRecord>> GetHistoryByItemIdAsync(int itemId, CancellationToken cancellationToken)
    {
        const string existsSql = """
            select exists (
                select 1
                from items
                where item_id = @ItemId
            );
            """;
        const string sql = """
            select
                o.order_id as OrderId,
                o.start_date as StartDate,
                o.end_date as EndDate,
                bd.borrower_name as Name,
                o.status as Status
            from orders o
            left join borrower_details bd on bd.borrower_detail_id = o.borrower_detail_id
            where o.item_id = @ItemId
            order by o.start_date desc;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        var itemExists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(existsSql, new { ItemId = itemId }, cancellationToken: cancellationToken));
        if (!itemExists)
        {
            return [];
        }

        var records = (await connection.QueryAsync<LoanRecord>(
            new CommandDefinition(sql, new { ItemId = itemId }, cancellationToken: cancellationToken))).ToArray();

        return records.Length == 0 ? [new LoanRecord(null, null, null, null, null)] : records;
    }

    private async Task<UserLoan?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                o.order_id as OrderId,
                coalesce(bd.user_id, 0) as UserId,
                o.start_date as OrderStartDate,
                o.end_date as OrderEndDate,
                o.status as OrderStatus,
                o.order_id as ObjectDetailId,
                o.item_id as ObjectId,
                coalesce(i.object_name, '') as ObjectName,
                o.status as DetailStatus,
                o.actual_return_date as ActualReturnDate
            from orders o
            left join borrower_details bd on bd.borrower_detail_id = o.borrower_detail_id
            left join items i on i.item_id = o.item_id
            where o.order_id = @OrderId;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<UserLoanRow>(
            new CommandDefinition(sql, new { OrderId = orderId }, cancellationToken: cancellationToken));
        return row is null ? null : Map(row);
    }

    private async Task<Result<BorrowerDetailEntity>> GetOrCreateBorrowerDetailAsync(
        int? borrowerId,
        string? borrowerName,
        int? ownerId,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        UserEntity? borrower = null;
        if (borrowerId is not null)
        {
            borrower = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == borrowerId && !x.IsDeleted, cancellationToken);
            if (borrower is null)
            {
                return Result<BorrowerDetailEntity>.Failure(LoanRepositoryErrors.BorrowerNotFound(borrowerId.Value));
            }
        }

        var displayName = borrower?.DisplayName ?? borrowerName?.Trim() ?? "";
        var existing = await db.BorrowerDetails
            .FirstOrDefaultAsync(
                x => x.UserId == borrowerId
                    && x.BorrowerName == displayName,
                cancellationToken);
        if (existing is not null)
        {
            existing.UpdatedAt = today;
            existing.UpdatedBy = ownerId?.ToString() ?? string.Empty;
            return Result<BorrowerDetailEntity>.Success(existing);
        }

        var detail = new BorrowerDetailEntity
        {
            UserId = borrowerId,
            BorrowerName = displayName,
            CreatedAt = today,
            UpdatedAt = today,
            CreatedBy = ownerId?.ToString() ?? string.Empty,
            UpdatedBy = ownerId?.ToString() ?? string.Empty
        };

        db.BorrowerDetails.Add(detail);
        await db.SaveChangesAsync(cancellationToken);
        return Result<BorrowerDetailEntity>.Success(detail);
    }

    private static UserLoan Map(UserLoanRow row) => new(
        row.OrderId,
        row.UserId,
        row.OrderStartDate,
        row.OrderEndDate,
        row.OrderStatus,
        [
            new LoanItemDetail(
                row.ObjectDetailId,
                row.ObjectId,
                row.ObjectName,
                row.DetailStatus,
                row.ActualReturnDate)
        ]);

    private static DateOnly Today() => DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);

    private sealed record UserLoanRow(
        int OrderId,
        int UserId,
        DateOnly OrderStartDate,
        DateOnly OrderEndDate,
        string OrderStatus,
        int ObjectDetailId,
        int ObjectId,
        string ObjectName,
        string DetailStatus,
        DateOnly? ActualReturnDate);
}
