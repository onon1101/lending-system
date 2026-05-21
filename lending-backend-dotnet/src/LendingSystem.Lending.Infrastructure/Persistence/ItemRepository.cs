using LendingSystem.Lending.Application.Abstractions;
using LendingSystem.Lending.Domain.Items;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.Lending.Infrastructure.Persistence;

public sealed class ItemRepository(LendingDbContext db, IQueryConnectionFactory queryConnectionFactory) : IItemRepository
{
    public async Task<Item> CreateAsync(int userId, string objectName, string maker, string material, string description, string imageUrl, CancellationToken cancellationToken)
    {
        var entity = new ItemEntity
        {
            OwnerId = userId,
            ObjectName = objectName,
            Maker = maker,
            Material = material,
            Description = description,
            CurrentStatus = ItemStatuses.Available,
            ImageUrl = imageUrl
        };

        db.Items.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    public async Task<Item?> GetByIdAsync(int itemId, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                item_id as ItemId,
                owner_id as OwnerId,
                object_name as ObjectName,
                maker as Maker,
                material as Material,
                coalesce(description, '') as Description,
                coalesce(current_status, '') as CurrentStatus,
                image_url as ImageUrl
            from items
            where item_id = @ItemId;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Item>(
            new CommandDefinition(sql, new { ItemId = itemId }, cancellationToken: cancellationToken));
    }

    public async Task<Item?> GetByNameAsync(int userId, string itemName, CancellationToken cancellation)
    {
        const string sql = """
            select
                item_id as ItemId,
                owner_id as OwnerId,
                object_name as ObjectName,
                maker as Maker,
                material as Material,
                coalesce(description, '') as Description,
                coalesce(current_status, '') as CurrentStatus,
                image_url as ImageUrl
            from items
            where object_name = @ItemName
              and owner_id = @UserId;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Item>(
            new CommandDefinition(sql, new { UserId = userId, ItemName = itemName }, cancellationToken: cancellation));
    }

    public async Task<IReadOnlyCollection<ItemSummary>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            select
                i.item_id as ItemId,
                i.owner_id as OwnerId,
                i.object_name as ObjectName,
                i.maker as Maker,
                i.material as Material,
                coalesce(i.description, '') as Description,
                coalesce(i.current_status, '') as CurrentStatus,
                u.name as OwnerUsername,
                u.display_name as OwnerName,
                u.email as OwnerEmail,
                i.image_url as ImageUrl
            from items i
            left join users u on u.user_id = i.owner_id
            order by i.item_id;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        var items = await connection.QueryAsync<ItemSummary>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        return items.ToArray();
    }

    public async Task<IReadOnlyCollection<ItemSummary>?> GetItemsByUserId(int userId, CancellationToken cancellationToken)
    {
        const string existsSql = """
            select exists (
                select 1
                from users
                where user_id = @UserId
                  and is_deleted = false
            );
            """;
        const string sql = """
            select
                i.item_id as ItemId,
                i.owner_id as OwnerId,
                i.object_name as ObjectName,
                i.maker as Maker,
                i.material as Material,
                coalesce(i.description, '') as Description,
                coalesce(i.current_status, '') as CurrentStatus,
                u.name as OwnerUsername,
                u.display_name as OwnerName,
                u.email as OwnerEmail,
                i.image_url as ImageUrl
            from items i
            join users u on u.user_id = i.owner_id
            where i.owner_id = @UserId
            order by i.item_id;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        var exists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(existsSql, new { UserId = userId }, cancellationToken: cancellationToken));
        if (!exists)
        {
            return null;
        }

        var items = await connection.QueryAsync<ItemSummary>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
        return items.ToArray();
    }

    public async Task<IReadOnlyCollection<ItemSummary>?> GetItemsByUserName(string username, CancellationToken cancellationToken)
    {
        const string existsSql = """
            select exists (
                select 1
                from users
                where name = @Username
                  and is_deleted = false
            );
            """;
        const string sql = """
            select
                i.item_id as ItemId,
                i.owner_id as OwnerId,
                i.object_name as ObjectName,
                i.maker as Maker,
                i.material as Material,
                coalesce(i.description, '') as Description,
                coalesce(i.current_status, '') as CurrentStatus,
                u.name as OwnerUsername,
                u.display_name as OwnerName,
                u.email as OwnerEmail,
                i.image_url as ImageUrl
            from users u
            join items i on i.owner_id = u.user_id
            where u.name = @Username
              and u.is_deleted = false
            order by i.item_id;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        var exists = await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(existsSql, new { Username = username }, cancellationToken: cancellationToken));
        if (!exists)
        {
            return null;
        }

        var rows = (await connection.QueryAsync<ItemSummary>(
            new CommandDefinition(sql, new { Username = username }, cancellationToken: cancellationToken))).ToArray();

        return rows;
    }

    public async Task<Item?> UpdateAsync(int itemId, string? objectName, string? maker, string? material, string? description, string? currentStatus, string? imageUrl, CancellationToken cancellationToken)
    {
        var entity = await db.Items
            .FirstOrDefaultAsync(x => x.ItemId == itemId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(objectName))
        {
            entity.ObjectName = objectName;
        }

        if (maker is not null)
        {
            entity.Maker = maker;
        }

        if (material is not null)
        {
            entity.Material = material;
        }

        if (!string.IsNullOrEmpty(description))
        {
            entity.Description = description;
        }

        if (!string.IsNullOrEmpty(currentStatus))
        {
            entity.CurrentStatus = currentStatus;
        }

        if (imageUrl is not null)
        {
            entity.ImageUrl = imageUrl;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    public async Task<IReadOnlyCollection<ItemMediaSummary>> GetMediaByItemIdAsync(int itemId, CancellationToken cancellationToken)
    {
        const string sql = """
            select
                im.type as Type,
                null::text as Creator,
                coalesce(im.description, '') as Description,
                coalesce(im.link, '') as OriginalLink,
                im.url as Media,
                im.created_at as CreatedAt
            from item_media im
            where im.item_id = @ItemId

            union all

            select
                lm.type as Type,
                coalesce(u.display_name, bd.borrower_name) as Creator,
                coalesce(lm.description, '') as Description,
                coalesce(lm.link, '') as OriginalLink,
                lm.url as Media,
                lm.created_at as CreatedAt
            from lending_media lm
            join orders o on o.order_id = lm.order_id
            left join borrower_details bd on bd.borrower_detail_id = o.borrower_detail_id
            left join users u on u.user_id = bd.user_id
            where o.item_id = @ItemId
            order by CreatedAt desc;
            """;

        using var connection = queryConnectionFactory.CreateConnection();
        var media = await connection.QueryAsync<ItemMediaRow>(
            new CommandDefinition(sql, new { ItemId = itemId }, cancellationToken: cancellationToken));
        return media.Select(x => new ItemMediaSummary(
            x.Type,
            x.Creator,
            x.Description,
            x.OriginalLink,
            x.Media,
            ToDateTimeOffset(x.CreatedAt))).ToArray();
    }

    private static Item Map(ItemEntity entity) => new(
        entity.ItemId,
        entity.OwnerId,
        entity.ObjectName,
        entity.Maker,
        entity.Material,
        entity.Description ?? "",
        entity.CurrentStatus ?? "",
        entity.ImageUrl);

    private static DateTimeOffset ToDateTimeOffset(DateTime? value) =>
        value is null ? default : new DateTimeOffset(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc));

    private sealed record ItemMediaRow(
        string Type,
        string? Creator,
        string Description,
        string OriginalLink,
        string Media,
        DateTime? CreatedAt);
}
