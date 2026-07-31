using Microsoft.AspNetCore.Http;

namespace LendingSystem.Lending.WebApi.CreateItem;

/// <summary>
/// 建立物品請求，但有附帶圖片
/// </summary>
/// <param name="ObjectName"></param>
/// <param name="Maker"></param>
/// <param name="Material"></param>
/// <param name="Description"></param>
/// <param name="File"></param>
public sealed record CreateItemWithImageRequest(
    string ObjectName,
    string? Maker,
    string? Material,
    string? Description,
    IFormFile? File);