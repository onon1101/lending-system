using LendingSystem.Application.Abstractions;
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

    public async Task<UserLoan> CreateAsync(int userId, IReadOnlyCollection<int> itemIds, int durationHours, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var tx = await connection.BeginTransactionAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var endTime = now.AddHours(durationHours);

        await using var orderCommand = new NpgsqlCommand("""
            INSERT INTO orders (user_id, start_time, end_time, status)
            VALUES (@user_id, @start_time, @end_time, 'On Loan')
            RETURNING order_id;
            """, connection, tx);
        orderCommand.Parameters.AddWithValue("user_id", userId);
        orderCommand.Parameters.AddWithValue("start_time", now);
        orderCommand.Parameters.AddWithValue("end_time", endTime);
        var orderId = (int)(await orderCommand.ExecuteScalarAsync(cancellationToken) ?? 0);

        foreach (var objectId in itemIds)
        {
            await using var detailCommand = new NpgsqlCommand("""
                INSERT INTO order_details (order_id, object_id, detail_status)
                VALUES (@order_id, @object_id, 'On Loan');
                """, connection, tx);
            detailCommand.Parameters.AddWithValue("order_id", orderId);
            detailCommand.Parameters.AddWithValue("object_id", objectId);
            await detailCommand.ExecuteNonQueryAsync(cancellationToken);

            await using var itemCommand = new NpgsqlCommand("""
                UPDATE items
                SET current_status = 'On Loan'
                WHERE object_id = @object_id AND current_status = 'Available';
                """, connection, tx);
            itemCommand.Parameters.AddWithValue("object_id", objectId);
            var rows = await itemCommand.ExecuteNonQueryAsync(cancellationToken);
            if (rows == 0)
            {
                await tx.RollbackAsync(cancellationToken);
                throw new InvalidOperationException($"物品 ID {objectId} 不可用或不存在，交易取消");
            }
        }

        await tx.CommitAsync(cancellationToken);

        return new UserLoan(orderId, userId, now, endTime, ItemStatuses.OnLoan, []);
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
}
