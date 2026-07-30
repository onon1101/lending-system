using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Media;
/// <summary>
/// 儲存圖片或影片等影音媒介
/// </summary>
public sealed class MediaEntity : Entity
{
    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="mediaId"></param>
    /// <param name="type"></param>
    /// <param name="url"></param>
    /// <param name="link"></param>
    /// <param name="description"></param>
    /// <param name="createdAt"></param>
    private MediaEntity(
        MediaId mediaId,
        MediaType type,
        string url,
        string? link,
        string? description,
        DateTimeOffset createdAt)
    {
        MediaId = mediaId;
        Type = type;
        Url = url;
        Link = link;
        Description = description;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// 索引
    /// </summary>
    public MediaId MediaId { get; }

    /// <summary>
    /// 影音類別
    /// </summary>
    public MediaType Type { get; }

    /// <summary>
    /// 影音連結
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// 原始影像連結
    /// </summary>
    public string? Link { get; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// 建立影音媒介
    /// </summary>
    public static Result<MediaEntity> Create(
        MediaId mediaId,
        MediaType type,
        string url,
        string? link = null,
        string? description = null,
        DateTimeOffset? createdAt = null)
    {


        if (string.IsNullOrWhiteSpace(url))
        {
            return Result<MediaEntity>.Failure(
                new Errors("Media.UrlRequired", "影音連結不可為空。"));
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
        {
            return Result<MediaEntity>.Failure(
                new Errors("Media.InvalidUrl", "影音連結格式不正確。"));
        }

        if (!string.IsNullOrWhiteSpace(link) &&
            !Uri.TryCreate(link, UriKind.Absolute, out _))
        {
            return Result<MediaEntity>.Failure(
                new Errors("Media.InvalidLink", "原始影像連結格式不正確。"));
        }

        var media = new MediaEntity(
            mediaId,
            type,
            url.Trim(),
            string.IsNullOrWhiteSpace(link) ? null : link.Trim(),
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            createdAt ?? DateTimeOffset.UtcNow);

        return Result<MediaEntity>.Success(media);
    }
}
