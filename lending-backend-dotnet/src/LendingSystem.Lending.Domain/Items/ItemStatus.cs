namespace LendingSystem.Lending.Domain.Item;

public sealed class ItemStatus : Enumeration<int, string>
{
    /// <summary>
    /// 可借用
    /// </summary>
    public static readonly ItemStatus Available =
        new(0, "Available");

    /// <summary>
    /// 正在借用中
    /// </summary>
    public static readonly ItemStatus OnLoan =
        new(1, "On Loan");

    /// <summary>
    /// 不可借用
    /// </summary>
    public static readonly ItemStatus UnAvailable =
        new(2, "Unavailable");

    public ItemStatus(int key, string value) : base(key, value) { }
}
