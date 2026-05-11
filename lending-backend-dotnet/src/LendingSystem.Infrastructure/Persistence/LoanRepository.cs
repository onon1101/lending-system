using LendingSystem.Application.Abstractions;
using LendingSystem.Application.Common;
using LendingSystem.Domain.Items;
using LendingSystem.Domain.Loans;
using Npgsql;

namespace LendingSystem.Infrastructure.Persistence;

public sealed class LoanRepository(NpgsqlDataSource dataSource) : ILoanRepository
{
    public async Task<IReadOnlyCollection<UserLoan>> GetActiveLoansByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT o.order_id, o.start_time, o.end_time, o.status AS order_status,
                   od.object_detail_id, od.object_id, i.object_name, od.detail_status, od.actual_return_time
            FROM orders o
            JOIN order_details od ON o.order_id = od.order_id
            JOIN items i ON od.object_id = i.object_id
            WHERE o.user_id = @user_id AND o.status = 'On Loan'
            ORDER BY o.order_id, i.object_id;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("user_id", userId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var grouped = new Dictionary<int, LoanBuilder>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var orderId = reader.GetInt32(0);
            if (!grouped.TryGetValue(orderId, out var loan))
            {
                loan = new LoanBuilder(
                    orderId,
                    userId,
                    reader.GetFieldValue<DateTimeOffset>(1),
                    reader.GetFieldValue<DateTimeOffset>(2),
                    reader.GetString(3));
                grouped.Add(orderId, loan);
            }

            loan.Items.Add(new LoanItemDetail(
                reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetString(6),
                reader.GetString(7),
                reader.IsDBNull(8) ? null : reader.GetFieldValue<DateTimeOffset>(8)));
        }

        return grouped.Values.Select(x => x.Build()).ToArray();
    }

    public async Task<Result<UserLoan>> CreateAsync(int userId, IReadOnlyCollection<int> itemIds, int durationHours, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var tx = await connection.BeginTransactionAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var endTime = now.AddHours(durationHours);

        await using var orderCommand = new NpgsqlCommand("""
            INSERT INTO orders (user_id, start_time, end_time, status)
            VALUES (@user_id, @start_time, @end_time, @status)
            RETURNING order_id;
            """, connection, tx);
        orderCommand.Parameters.AddWithValue("user_id", userId);
        orderCommand.Parameters.AddWithValue("start_time", now);
        orderCommand.Parameters.AddWithValue("end_time", endTime);
        orderCommand.Parameters.AddWithValue("status", LoanStatuses.OnLoan);
        var orderId = (int)(await orderCommand.ExecuteScalarAsync(cancellationToken) ?? 0);

        foreach (var objectId in itemIds)
        {
            await using var detailCommand = new NpgsqlCommand("""
                INSERT INTO order_details (order_id, object_id, detail_status)
                VALUES (@order_id, @object_id, @status);
                """, connection, tx);
            detailCommand.Parameters.AddWithValue("order_id", orderId);
            detailCommand.Parameters.AddWithValue("object_id", objectId);
            detailCommand.Parameters.AddWithValue("status", LoanStatuses.OnLoan);
            await detailCommand.ExecuteNonQueryAsync(cancellationToken);

            await using var itemCommand = new NpgsqlCommand("""
                UPDATE items
                SET current_status = @on_loan
                WHERE object_id = @object_id AND current_status = @available;
                """, connection, tx);
            itemCommand.Parameters.AddWithValue("object_id", objectId);
            itemCommand.Parameters.AddWithValue("on_loan", LoanStatuses.OnLoan);
            itemCommand.Parameters.AddWithValue("available", ItemStatuses.Available);
            var rows = await itemCommand.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0)
            {
                await tx.RollbackAsync(cancellationToken);
                return Result<UserLoan>.Failure(ErrorCodes.Conflict, $"物品 ID {objectId} 不可用或不存在，交易取消");
            }
        }

        await tx.CommitAsync(cancellationToken);

        return Result<UserLoan>.Success(new UserLoan(orderId, userId, now, endTime, LoanStatuses.OnLoan, []));
    }

    public async Task<Result<UserLoan>> ReturnItemAsync(int orderId, int objectId, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var tx = await connection.BeginTransactionAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;

        await using (var detailCommand = new NpgsqlCommand("""
            UPDATE order_details
            SET detail_status = @returned,
                actual_return_time = @actual_return_time
            WHERE order_id = @order_id
              AND object_id = @object_id
              AND detail_status = @on_loan;
            """, connection, tx))
        {
            detailCommand.Parameters.AddWithValue("returned", LoanStatuses.Returned);
            detailCommand.Parameters.AddWithValue("actual_return_time", now);
            detailCommand.Parameters.AddWithValue("order_id", orderId);
            detailCommand.Parameters.AddWithValue("object_id", objectId);
            detailCommand.Parameters.AddWithValue("on_loan", LoanStatuses.OnLoan);
            var rows = await detailCommand.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0)
            {
                await tx.RollbackAsync(cancellationToken);
                return Result<UserLoan>.Failure(ErrorCodes.Conflict, $"借閱單 {orderId} 中的物品 ID {objectId} 不存在或已歸還");
            }
        }

        await using (var itemCommand = new NpgsqlCommand("""
            UPDATE items
            SET current_status = @available
            WHERE object_id = @object_id;
            """, connection, tx))
        {
            itemCommand.Parameters.AddWithValue("available", ItemStatuses.Available);
            itemCommand.Parameters.AddWithValue("object_id", objectId);
            await itemCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var orderCommand = new NpgsqlCommand("""
            UPDATE orders
            SET status = @returned
            WHERE order_id = @order_id
              AND NOT EXISTS (
                  SELECT 1
                  FROM order_details
                  WHERE order_id = @order_id AND detail_status = @on_loan
              );
            """, connection, tx))
        {
            orderCommand.Parameters.AddWithValue("returned", LoanStatuses.Returned);
            orderCommand.Parameters.AddWithValue("order_id", orderId);
            orderCommand.Parameters.AddWithValue("on_loan", LoanStatuses.OnLoan);
            await orderCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);

        var loan = await GetByOrderIdAsync(orderId, cancellationToken);
        return loan is null
            ? Result<UserLoan>.Failure(ErrorCodes.NotFound, "Loan not found")
            : Result<UserLoan>.Success(loan);
    }

    public async Task<IReadOnlyCollection<LoanRecord>> GetHistoryByItemIdAsync(int itemId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT c.start_time, c.end_time, d.name, c.status
            FROM items a
            LEFT JOIN order_details b ON b.object_id = a.object_id
            LEFT JOIN orders c ON c.order_id = b.order_id
            LEFT JOIN users d ON d.user_id = c.user_id
            WHERE a.object_id = @object_id
            ORDER BY c.start_time DESC;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("object_id", itemId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<LoanRecord>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new LoanRecord(
                reader.IsDBNull(0) ? null : reader.GetFieldValue<DateTimeOffset>(0),
                reader.IsDBNull(1) ? null : reader.GetFieldValue<DateTimeOffset>(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3)));
        }

        return result;
    }

    private sealed record LoanBuilder(int OrderId, int UserId, DateTimeOffset StartTime, DateTimeOffset EndTime, string Status)
    {
        public List<LoanItemDetail> Items { get; } = [];

        public UserLoan Build() => new(OrderId, UserId, StartTime, EndTime, Status, Items);
    }

    private async Task<UserLoan?> GetByOrderIdAsync(int orderId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT o.order_id, o.user_id, o.start_time, o.end_time, o.status AS order_status,
                   od.object_detail_id, od.object_id, i.object_name, od.detail_status, od.actual_return_time
            FROM orders o
            JOIN order_details od ON o.order_id = od.order_id
            JOIN items i ON od.object_id = i.object_id
            WHERE o.order_id = @order_id
            ORDER BY i.object_id;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("order_id", orderId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        LoanBuilder? loan = null;
        while (await reader.ReadAsync(cancellationToken))
        {
            loan ??= new LoanBuilder(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetFieldValue<DateTimeOffset>(2),
                reader.GetFieldValue<DateTimeOffset>(3),
                reader.GetString(4));

            loan.Items.Add(new LoanItemDetail(
                reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                reader.GetInt32(6),
                reader.GetString(7),
                reader.GetString(8),
                reader.IsDBNull(9) ? null : reader.GetFieldValue<DateTimeOffset>(9)));
        }

        return loan?.Build();
    }
}
