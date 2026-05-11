namespace LendingSystem.Application.Abstractions;

public sealed record StoredObject(string Url, string ObjectName);

public interface IObjectStorage
{
    Task<StoredObject> UploadItemImageAsync(Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken);
    Task<StoredObject> UploadItemVideoAsync(Stream stream, long size, string fileName, string contentType, CancellationToken cancellationToken);
    Task DeleteObjectAsync(string objectName, CancellationToken cancellationToken);
}
