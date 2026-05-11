using LendingSystem.Application.Abstractions;
using LendingSystem.Domain.Items;
using Npgsql;

namespace LendingSystem.Infrastructure.Persistence;

public sealed class ItemRepository(NpgsqlDataSource dataSource) : IItemRepository
{
    public async Task<Item> CreateAsync(string objectName, string description, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO items (object_name, description, current_status)
            VALUES (@object_name, @description, 'Available')
            RETURNING object_id, current_status;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("object_name", objectName);
        command.Parameters.AddWithValue("description", description);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new Item(reader.GetInt32(0), objectName, description, reader.GetString(1), null);
    }

    public async Task<Item?> GetByIdAsync(int objectId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT object_id, object_name, description, current_status, image_url
            FROM items
            WHERE object_id = @object_id;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("object_id", objectId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadItem(reader) : null;
    }

    public async Task<IReadOnlyCollection<ItemSummary>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT a.object_id, a.object_name, a.description, a.current_status,
                   b.name AS owner_name, b.email AS owner_email, a.image_url
            FROM items a
            LEFT JOIN users b ON b.user_id = a.owner_id
            ORDER BY a.object_id;
            """;

        await using var command = dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<ItemSummary>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new ItemSummary(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5),
                reader.IsDBNull(6) ? null : reader.GetString(6)));
        }

        return result;
    }

    public async Task<Item?> UpdateAsync(int objectId, string? objectName, string? description, string? currentStatus, string? imageUrl, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE items SET
                object_name = COALESCE(NULLIF(@object_name, ''), object_name),
                description = COALESCE(NULLIF(@description, ''), description),
                current_status = COALESCE(NULLIF(@current_status, ''), current_status),
                image_url = COALESCE(@image_url, image_url)
            WHERE object_id = @object_id
            RETURNING object_id, object_name, description, current_status, image_url;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("object_id", objectId);
        command.Parameters.AddWithValue("object_name", (object?)objectName ?? DBNull.Value);
        command.Parameters.AddWithValue("description", (object?)description ?? DBNull.Value);
        command.Parameters.AddWithValue("current_status", (object?)currentStatus ?? DBNull.Value);
        command.Parameters.AddWithValue("image_url", (object?)imageUrl ?? DBNull.Value);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadItem(reader) : null;
    }

    public async Task<IReadOnlyCollection<ItemMediaSummary>> GetMediaByItemIdAsync(int itemId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT a.type, c.name, a.description, a.link, a.url, a.created_at
            FROM media a
            LEFT JOIN orders b ON b.order_id = a.order_id
            LEFT JOIN users c ON c.user_id = b.user_id
            WHERE a.object_id = @object_id;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("object_id", itemId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var result = new List<ItemMediaSummary>();
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new ItemMediaSummary(
                reader.GetString(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? "" : reader.GetString(3),
                reader.GetString(4),
                reader.GetFieldValue<DateTimeOffset>(5)));
        }

        return result;
    }

    private static Item ReadItem(NpgsqlDataReader reader) => new(
        reader.GetInt32(0),
        reader.GetString(1),
        reader.GetString(2),
        reader.GetString(3),
        reader.IsDBNull(4) ? null : reader.GetString(4));
}
