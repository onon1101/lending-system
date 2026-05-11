namespace LendingSystem.Domain.Media;

public static class MediaTypes
{
    public const string Image = "image";
    public const string Video = "video";
}

public sealed record MediaAsset(
    int MediaId,
    int? OrderId,
    int ObjectId,
    string Type,
    string Description,
    string Url,
    string Link,
    DateTimeOffset CreatedAt);
