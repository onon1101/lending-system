namespace LendingSystem.Application.Items;

public sealed record FileFormat(Stream Stream, long Size, string FileName, string ContentType);
