namespace LendingSystem.SharedKernel.Infrastructure.Persistence;

public sealed class UserEntity
{
    public int UserId { get; init; }
    public required string Name { get; init; }
    public string DisplayName { get; set; } = "";
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string AuthProvider { get; set; } = "local";
    public string? ProviderUserId { get; set; }
    public bool IsDeleted { get; set; }
    // public string? Nickname { get; set; }
    public string Role { get; set; } = "user"; // user or admin 
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// 抱枕套，在某個使用者的情況底下
/// </summary>
/// <remarks>
/// 不同的使用者，可能會擁有同一個款式的抱枕套
/// </remarks>
public sealed class ItemEntity
{
    /// <summary>
    /// Pk
    /// </summary>
    public int ItemId { get; set; }

    /// <summary>
    /// 物品所屬者 Id
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// 物品名稱
    /// </summary>
    public string ObjectName { get; set; } = "";

    /// <summary>
    /// 借月狀態
    /// </summary>
    public string CurrentStatus { get; set; } = "Available";
    public string? ImageUrl { get; set; }
    public string Description { get; set; } = "";

    public UserEntity? Owner { get; set; }

    /// <summary>
    /// 製作抱枕的畫師
    /// </summary>
    public string Maker { get; set; } = string.Empty;

    /// <summary>
    /// 材質
    /// </summary>
    public string Material { get; set; } = string.Empty;

    public ICollection<OrderEntity> Orders { get; set; } = [];
    public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class OrderEntity
{
    public int OrderId { get; set; }
    public int BorrowerDetailId { get; set; }
    public int ObjectId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly? ActualReturnDate { get; set; }
    public string Status { get; set; } = "";
    public BorrowerDetailEntity? BorrowerDetail { get; set; }
    public ItemEntity? Item { get; set; }
    public ICollection<MediaEntity> Media { get; set; } = [];
}

public sealed class BorrowerDetailEntity 
{
    public int BorrowerDetailId { get; set; }
    /// <summary>
    /// 借閱者的Id
    /// </summary>
    /// <remarks>
    /// 如果借閱者為系統已註冊的使用者，則此帶入 UserId，否則為空。
    /// </remarks>
    public int? UserId { get; set; }
    public string BorrowerName { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateOnly CreatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public DateOnly UpdatedAt { get; set; }
    public UserEntity? User { get; set; }
    public ICollection<OrderEntity> Orders { get; set; } = [];
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
