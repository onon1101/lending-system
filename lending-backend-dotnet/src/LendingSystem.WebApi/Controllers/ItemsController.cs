using LendingSystem.Lending.Application.Items;
using LendingSystem.Lending.Application.Media;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class ItemsController(ItemService items) : ControllerBase
{
    [HttpGet("/api/v1/catalog/items")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ItemSummaryResponse>>>> GetAll(CancellationToken cancellationToken) =>
        this.ToActionResult(await items.GetAllAsync(cancellationToken));

    /// <summary>
    /// 取得某個使用者所擁有的所有物品
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/catalog/items/user/{userId:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ItemSummaryResponse>>>> GetItemsByUserId(
        [FromRoute] int userId, CancellationToken cancellationToken)
        => this.ToActionResult(await items.GetItemsByUserId(userId, cancellationToken));

    /// <summary>
    /// 依照使用者名稱取得該使用者擁有的所有物品
    /// </summary>
    /// <param name="username"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/catalog/items/user/{username}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ItemSummaryResponse>>>> GetItemsByUserName(
        [FromRoute] string username, CancellationToken cancellationToken)
        => this.ToActionResult(await items.GetItemsByUserName(username, cancellationToken));
        

    [HttpPost("/api/v1/catalog/items")]
    [Authorize(Roles = "admin,user")]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> Create(
        [FromBody] CreateItemRequest request,
        CancellationToken cancellationToken) =>
        await CreateForCurrentUserAsync(request, null, cancellationToken);

    [HttpPost("/api/v1/catalog/items/form")]
    [Authorize(Roles = "admin,user")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> CreateWithForm(
        [FromForm] CreateItemFormRequest form,
        CancellationToken cancellationToken)
    {
        var request = new CreateItemRequest(form.ObjectName, form.Maker, form.Material, form.Description);
        return await CreateForCurrentUserAsync(request, form.Image, cancellationToken);
    }

    private async Task<ActionResult<ApiResponse<ItemResponse>>> CreateForCurrentUserAsync(
        CreateItemRequest request,
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return this.ApiFailure<ItemResponse>(ControllerApiErrors.TokenInvalid());
        }

        await using var stream = image?.OpenReadStream();
        var fileFormat = image is null ? null : new FileFormat(stream!, image.Length, image.FileName, image.ContentType);
        var created = await items.CreateAsync(request, userId, fileFormat, cancellationToken);

        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/catalog/items/{created.Data!.ItemId}" : "", created);
    }

    [HttpGet("/api/v1/catalog/items/{objectId:int}")]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> GetById([FromRoute] int objectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await items.GetByIdAsync(objectId, cancellationToken));

    [HttpGet("/api/v1/catalog/users/{userId:int}/items/{objectName}")]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> GetByName(
        [FromRoute] int userId,
        [FromRoute] string objectName,
        CancellationToken cancellation) =>
        this.ToActionResult(await items.GetByNameAsync(userId, objectName, cancellation));


    [HttpPut("/api/v1/catalog/items/{objectId:int}")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> Update([FromRoute] int objectId, [FromBody] UpdateItemRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return this.ApiFailure<ItemResponse>(ControllerApiErrors.TokenInvalid());
        }

        return this.ToActionResult(await items.UpdateAsync(objectId, request, userId, IsAdmin(User), cancellationToken));
    }

    [HttpPost("/api/v1/catalog/items/{objectId:int}/image")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> UploadImage([FromRoute] int objectId, IFormFile file, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return this.ApiFailure<ItemResponse>(ControllerApiErrors.TokenInvalid());
        }

        await using var stream = file.OpenReadStream();
        return this.ToActionResult(await items.UploadImageAsync(objectId, new FileFormat(stream, file.Length, file.FileName, file.ContentType), userId, IsAdmin(User), cancellationToken));
    }

    [HttpPost("/api/v1/catalog/items/media")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<MediaResponse>>> UploadMedia(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return this.ApiFailure<MediaResponse>(ControllerApiErrors.TokenInvalid());
        }

        var file = Request.Form.Files["file"];
        if (file is null)
        {
            return this.ApiFailure<MediaResponse>(ControllerApiErrors.MissingFiles());
        }

        if (!int.TryParse(Request.Form["object_id"].ToString(), out var objectId))
        {
            return this.ApiFailure<MediaResponse>(ControllerApiErrors.MissingField(missingField: "object_id"));
        }

        var orderIdValue = Request.Form["order_id"].ToString();
        int? orderId = null;
        if (!string.IsNullOrWhiteSpace(orderIdValue) && orderIdValue != "0")
        {
            if (!int.TryParse(orderIdValue, out var parsedOrderId))
            {
                return this.ApiFailure<MediaResponse>(ControllerApiErrors.MustBeInteger(missingField: "order_id"));
            }

            orderId = parsedOrderId;
        }

        await using var stream = file.OpenReadStream();

        var result = await items.UploadMediaAsync(
            orderId,
            objectId,
            Request.Form["description"].ToString(),
            Request.Form["link"].ToString(),
            stream,
            file.Length,
            file.FileName,
            file.ContentType,
            userId,
            IsAdmin(User),
            cancellationToken);

        return this.ToCreatedActionResult($"/api/v1/catalog/items/{objectId}/media", result);
    }

    [HttpGet("/api/v1/catalog/items/{objectId:int}/media")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ItemMediaResponse>>>> GetMedia([FromRoute] int objectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await items.GetMediaAsync(objectId, cancellationToken));

    private static bool TryGetUserId(ClaimsPrincipal user, out int userId)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("id");
        return int.TryParse(value, out userId);
    }

    private static bool IsAdmin(ClaimsPrincipal user) => user.IsInRole("admin");
}

public sealed class CreateItemFormRequest
{
    [FromForm(Name = "object_name")]
    public string ObjectName { get; init; } = "";

    [FromForm(Name = "maker")]
    public string? Maker { get; init; }

    [FromForm(Name = "material")]
    public string? Material { get; init; }

    [FromForm(Name = "description")]
    public string Description { get; init; } = "";

    [FromForm(Name = "image")]
    public IFormFile? Image { get; init; }
}
