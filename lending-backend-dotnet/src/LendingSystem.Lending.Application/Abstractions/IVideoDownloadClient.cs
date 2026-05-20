namespace LendingSystem.Lending.Application.Abstractions;

public sealed record VideoChunk(byte[]? Bytes, int? Progress, string? ErrorMessage, string? FileName);

public interface IVideoDownloadClient
{
    IAsyncEnumerable<VideoChunk> DownloadAndStreamAsync(Uri sourceUrl, CancellationToken cancellationToken);
}
