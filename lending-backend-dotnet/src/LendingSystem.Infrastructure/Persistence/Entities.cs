namespace LendingSystem.Infrastructure.Persistence;

public sealed class UserEntity
{
    public int UserId { get; set; }
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsDeleted { get; set; }
    public string? Nickname { get; set; }
    public string? Role { get; set; } = "user";
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public ICollection<OrderEntity> Orders { get; set; } = [];
}

public sealed class ItemEntity
{
    public int ObjectId { get; set; }
    public string ObjectName { get; set; } = "";
    public string? Description { get; set; }
    public string? CurrentStatus { get; set; } = "Available";
    public int? OwnerId { get; set; }
    public string? ImageUrl { get; set; }
    public ICollection<OrderDetailEntity> OrderDetails { get; set; } = [];
    public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class OrderEntity
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string Status { get; set; } = "";
    public UserEntity? User { get; set; }
    public ICollection<OrderDetailEntity> Details { get; set; } = [];
    public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class OrderDetailEntity
{
    public int ObjectDetailId { get; set; }
    public int OrderId { get; set; }
    public int ObjectId { get; set; }
    public string DetailStatus { get; set; } = "";
    public DateTimeOffset? ActualReturnTime { get; set; }
    public OrderEntity? Order { get; set; }
    public ItemEntity? Item { get; set; }
}

public sealed class MediaEntity
{
    public int MediaId { get; set; }
    public int? OrderId { get; set; }
    public int ObjectId { get; set; }
    public string Type { get; set; } = "";
    public string Url { get; set; } = "";
    public string? Link { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
    public OrderEntity? Order { get; set; }
    public ItemEntity? Item { get; set; }
}
