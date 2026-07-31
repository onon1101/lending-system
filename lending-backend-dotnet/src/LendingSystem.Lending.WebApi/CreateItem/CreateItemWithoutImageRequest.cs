using System.ComponentModel.DataAnnotations;

namespace LendingSystem.Lending.WebApi.CreateItem;

/// <summary>
/// 建立物品。但不建立物品預覽圖
/// </summary>
/// <param name="ObjectName">物品名稱</param>
/// <param name="Maker">作者</param>
/// <param name="Material">材質</param>
/// <param name="Description">描述</param>
public sealed record CreateItemWithoutImageRequest(
    [property: Required] string ObjectName,
    string? Maker,
    string? Material,
    string? Description);