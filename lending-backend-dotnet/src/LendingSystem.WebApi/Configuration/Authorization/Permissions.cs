namespace LendingSystem.WebApi.Configuration.Authorization;

public static class Permissions
{
    public const string ReadUsers = "users:read";
    public const string DeleteUsers = "users:delete";

    public const string CreateItems = "items:create";
    public const string UpdateItems = "items:update";
    public const string UploadItemMedia = "items:media:upload";

    public const string ReadBorrowings = "borrowings:read";
    public const string CreateBorrowings = "borrowings:create";
    public const string ReturnBorrowings = "borrowings:return";
    public const string ManageBorrowings = "borrowings:manage";
}
