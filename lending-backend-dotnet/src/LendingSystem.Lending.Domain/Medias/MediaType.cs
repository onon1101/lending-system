namespace LendingSystem.Lending.Domain.Media;

public sealed class MediaType : Enumeration<int, string>
{
    public static readonly MediaType Photo =
        new(0, "照片");

    public static readonly MediaType Video =
        new(1, "影片");

    public MediaType(int key, string value) : base(key, value)
    {
    }
}
