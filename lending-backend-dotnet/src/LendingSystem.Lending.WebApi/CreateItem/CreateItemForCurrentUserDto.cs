using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LendingSystem.Lending.WebApi.CreateItem;

/// <summary>
/// 建立物品
/// </summary>
/// <param name="ObjectName"></param>
/// <param name="Maker"></param>
/// <param name="Material"></param>
/// <param name="Description"></param>
/// <param name="Image"></param>
public sealed record CreateItemForCurrentUserDto(
[property: Required] string ObjectName,
string? Maker,
string? Material,
string? Description,
    IFormFile? Image);