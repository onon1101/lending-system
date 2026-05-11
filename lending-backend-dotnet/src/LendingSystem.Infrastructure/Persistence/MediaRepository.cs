using LendingSystem.Application.Abstractions;
using LendingSystem.Domain.Media;
using Npgsql;

namespace LendingSystem.Infrastructure.Persistence;

public sealed class MediaRepository(NpgsqlDataSource dataSource) : IMediaRepository
{
    public async Task<MediaAsset> CreateAsync(int? orderId, int objectId, string type, string url, string link, string description, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO media (order_id, object_id, type, url, link, description)
            VALUES (@order_id, @object_id, @type, @url, @link, @description)
            RETURNING media_id, created_at;
            """;

        await using var command = dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("order_id", (object?)orderId ?? DBNull.Value);
        command.Parameters.AddWithValue("object_id", objectId);
        command.Parameters.AddWithValue("type", type);
        command.Parameters.AddWithValue("url", url);
        command.Parameters.AddWithValue("link", link);
        command.Parameters.AddWithValue("description", description);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);

        return new MediaAsset(
            reader.GetInt32(0),
            orderId,
            objectId,
            type,
            description,
            url,
            link,
            reader.GetFieldValue<DateTimeOffset>(1));
    }
}
