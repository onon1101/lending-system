using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Lending.Application.Items.CreateItem;
/// <summary>
/// 建立物品
/// </summary>
/// <param name="ObjectName">物品名稱</param>
/// <param name="Maker">作者</param>
/// <param name="Material">材質</param>
/// <param name="Description">描述</param>
/// <param name="UserId">使用者編號</param>
/// <param name="Image">物品圖像</param>
public sealed record CreateItemCommand(
    [property: Required] string ObjectName,
    string? Maker,
    string? Material,
    string? Description,
    long UserId,
    FileFormat? Image
) : IQuery<CreateItemResult>;