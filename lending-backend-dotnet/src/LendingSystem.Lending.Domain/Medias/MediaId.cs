using LendingSystem.SharedKernel.Domain.Common;

namespace LendingSystem.Lending.Domain.Media;

/// <summary>
/// 影音索引
/// </summary>
public sealed class MediaId : ValueObject<long>
{
    /// <summary>
    /// 建構
    /// </summary>
    /// <param name="id"></param>
    public MediaId(long id)
    {
        Id = id;
    }
    public long Id { get; set; }

    public static Result<MediaId> Create(long value)
    {
        if (value <= 0)
        {
            return Result<MediaId>.Failure(
                new Errors("Media.InvalidId", "影音索引必須大於 0。"));
        }

        var mediaId = new MediaId(value);

        return Result<MediaId>.Success(mediaId);
    }
    protected override IEnumerable<long> GetEqualityComponents()
    {
        yield return Id;
    }
}
