using LendingSystem.Application.Items;
using LendingSystem.Application.Media;
using LendingSystem.Application.Common;
using LendingSystem.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ItemSummaryResponse>>>> GetItemsByUserId(
        [FromRoute] int userId, CancellationToken cancellationToken)
        => this.ToActionResult(await items.GetItemsByUserId(userId, cancellationToken));
        

    //todo: 路徑改成 api/v1/catalog/items/
    [HttpPost("/api/v1/catalog/items")]
    [Authorize(Roles = "user")]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> Create([FromBody] CreateItemRequest request, CancellationToken cancellationToken)
    {
        var created = await items.CreateAsync(request, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/catalog/items/{created.Data!.ItemId}" : "", created);
    }

    [HttpGet("/api/v1/catalog/items/{objectId:int}")]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> GetById([FromRoute] int objectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await items.GetByIdAsync(objectId, cancellationToken));

    [HttpPut("/api/v1/catalog/items/{objectId:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> Update([FromRoute] int objectId, [FromBody] UpdateItemRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await items.UpdateAsync(objectId, request, cancellationToken));

    [HttpPost("/api/v1/catalog/items/{objectId:int}/image")]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> UploadImage([FromRoute] int objectId, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return this.ToActionResult(await items.UploadImageAsync(objectId, stream, file.Length, file.FileName, file.ContentType, cancellationToken));
    }

    [HttpPost("/api/v1/catalog/items/media")]
    public async Task<ActionResult<ApiResponse<MediaResponse>>> UploadMedia(CancellationToken cancellationToken)
    {
        var file = Request.Form.Files["file"];
        if (file is null)
        {
            return this.ApiFailure<MediaResponse>(ErrorCodes.Validation, "Missing File");
        }

        if (!int.TryParse(Request.Form["object_id"].ToString(), out var objectId))
        {
            return this.ApiFailure<MediaResponse>(ErrorCodes.Validation, "object_id is required");
        }

        var orderIdValue = Request.Form["order_id"].ToString();
        int? orderId = null;
        if (!string.IsNullOrWhiteSpace(orderIdValue) && orderIdValue != "0")
        {
            if (!int.TryParse(orderIdValue, out var parsedOrderId))
            {
                return this.ApiFailure<MediaResponse>(ErrorCodes.Validation, "order_id must be a number");
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
            cancellationToken);

        return this.ToCreatedActionResult($"/api/v1/catalog/items/{objectId}/media", result);
    }

    [HttpGet("/api/v1/catalog/items/{objectId:int}/media")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ItemMediaResponse>>>> GetMedia([FromRoute] int objectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await items.GetMediaAsync(objectId, cancellationToken));
}
