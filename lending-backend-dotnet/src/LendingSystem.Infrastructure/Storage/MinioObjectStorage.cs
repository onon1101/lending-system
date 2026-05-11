using LendingSystem.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace LendingSystem.Infrastructure.Storage;

public sealed class MinioObjectStorage(IMinioClient client, IConfiguration configuration) : IObjectStorage
{
    private string BucketName => configuration["MINIO_BUCKET_NAME"] ?? configuration["Minio:BucketName"] ?? "";
    private string PublicEndpoint => configuration["MINIO_ENDPOINT"] ?? configuration["Minio:Endpoint"] ?? "";

    public async Task<StoredObject> UploadItemImageAsync(Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        return await UploadAsync("item", stream, size, fileName, contentType, cancellationToken);
    }

    public async Task<StoredObject> UploadItemVideoAsync(Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        return await UploadAsync("video", stream, size, fileName, contentType, cancellationToken);
    }

    public async Task DeleteObjectAsync(string objectName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return;
        }

        await client.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(BucketName)
                .WithObject(objectName),
            cancellationToken);
    }

    private async Task<StoredObject> UploadAsync(string prefix, Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken)
    {
        await EnsureBucketAsync(cancellationToken);

        var objectName = $"{prefix}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{SanitizeFileName(fileName)}";
        await client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(BucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(size)
                .WithContentType(contentType),
            cancellationToken);

        var url = new UriBuilder(Uri.UriSchemeHttp, PublicEndpoint)
        {
            Path = $"{BucketName}/{objectName}"
        }.Uri.ToString();

        return new StoredObject(url, objectName);
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        var exists = await client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(BucketName),
            cancellationToken);

        if (exists)
        {
            return;
        }

        await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName), cancellationToken);
        var policy = $$"""{"Version":"2012-10-17","Statement":[{"Effect":"Allow","Principal":{"AWS":["*"]},"Action":["s3:GetObject"],"Resource":["arn:aws:s3:::{{BucketName}}/*"]}]}""";
        await client.SetPolicyAsync(new SetPolicyArgs().WithBucket(BucketName).WithPolicy(policy), cancellationToken);
    }

    private static string SanitizeFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        var baseName = Path.GetFileNameWithoutExtension(fileName)
            .Replace(" ", "_", StringComparison.Ordinal)
            .Replace(":", "-", StringComparison.Ordinal)
            .Replace(".", "_", StringComparison.Ordinal);

        return $"{baseName}{extension}";
    }
}
