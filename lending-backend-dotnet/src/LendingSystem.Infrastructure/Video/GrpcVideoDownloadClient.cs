using Grpc.Core;
using Grpc.Net.Client;
using LendingSystem.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Video;

namespace LendingSystem.Infrastructure.Video;

public sealed class GrpcVideoDownloadClient(IConfiguration configuration) : IVideoDownloadClient
{
    public async IAsyncEnumerable<VideoChunk> DownloadAndStreamAsync(Uri sourceUrl, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var address = configuration["VIDEO_SERVICE_ADDR"] ?? configuration["VideoService:Address"] ?? "192.168.2.236:50051";
        using var channel = GrpcChannel.ForAddress(NormalizeGrpcAddress(address));
        var client = new VideoService.VideoServiceClient(channel);

        using var call = client.DownloadAndStream(new DownloadRequest { Url = sourceUrl.ToString() }, cancellationToken: cancellationToken);
        while (await call.ResponseStream.MoveNext(cancellationToken))
        {
            var current = call.ResponseStream.Current;
            yield return current.PayloadCase switch
            {
                DownloadResponse.PayloadOneofCase.FileChunk => new VideoChunk(current.FileChunk.ToByteArray(), null, null, current.Filename),
                DownloadResponse.PayloadOneofCase.Progress => new VideoChunk(null, current.Progress, null, current.Filename),
                DownloadResponse.PayloadOneofCase.ErrorMes => new VideoChunk(null, null, current.ErrorMes, current.Filename),
                _ => new VideoChunk(null, null, null, current.Filename)
            };
        }
    }

    private static string NormalizeGrpcAddress(string address) =>
        address.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || address.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? address
            : $"http://{address}";
}
