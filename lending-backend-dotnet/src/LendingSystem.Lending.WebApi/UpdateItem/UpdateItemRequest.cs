namespace LendingSystem.Lending.WebApi.UpdateItem;

/// <summary>
/// 更新物品請求
/// </summary>
/// <param name="ObjectName">物品名稱</param>
/// <param name="Maker">作者</param>
/// <param name="Material">材質</param>
/// <param name="Description">描述</param>
/// <param name="CurrentStatus">當前狀態</param>
/// <param name="OriginUrl">來源網址</param>
public sealed record UpdateItemRequest(
    string ObjectName,
    string? Maker,
    string? Material,
    string? Description,
    string? CurrentStatus,
    string? OriginUrl);