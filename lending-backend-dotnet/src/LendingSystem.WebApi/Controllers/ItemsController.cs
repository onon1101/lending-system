using LendingSystem.Application.Items;
using LendingSystem.Application.Media;
using LendingSystem.Application.Common;
using LendingSystem.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

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
        

    [HttpPost("/api/v1/catalog/items")]
    [Authorize(Roles = "user")]
    public async Task<ActionResult<ApiResponse<ItemResponse>>> Create(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(User, out var userId))
        {
            return this.ApiFailure<ItemResponse>(ErrorCodes.Unauthorized, "Invalid token");
        }

        var input = await ReadCreateItemInputAsync(cancellationToken);
        if (!input.IsSuccess)
        {
            return this.ApiFailure<ItemResponse>(input.Error.Code, input.Error.Message);
        }

        var (request, file) = input.Data!;
        await using var stream = file?.OpenReadStream();
        var fileFormat = file is null ? null : new FileFormat(stream!, file.Length, file.FileName, file.ContentType);
        var created = await items.CreateAsync(request, userId, fileFormat, cancellationToken);

        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/catalog/items/{created.Data!.ItemId}" : "", created);
    }

    private async Task<Result<CreateItemInput>> ReadCreateItemInputAsync(CancellationToken cancellationToken)
    {
        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync(cancellationToken);
            return Result<CreateItemInput>.Success(new CreateItemInput(
                new CreateItemRequest(
                    GetFormValue(form, "object_name", "objectName"),
                    GetFormValue(form, "maker"),
                    GetFormValue(form, "material"),
                    GetFormValue(form, "description")),
                GetCoverFile(form.Files)));
        }

        try
        {
            var request = await JsonSerializer.DeserializeAsync<CreateItemRequest>(
                Request.Body,
                JsonOptions,
                cancellationToken);

            return request is null
                ? Result<CreateItemInput>.Failure(ErrorCodes.Validation, "Invalid request body")
                : Result<CreateItemInput>.Success(new CreateItemInput(request, null));
        }
        catch (JsonException)
        {
            return Result<CreateItemInput>.Failure(ErrorCodes.Validation, "Invalid request body");
        }
    }

    private static string GetFormValue(IFormCollection form, string name, string? alternativeName = null)
    {
        var value = form[name].ToString();
        return string.IsNullOrEmpty(value) && alternativeName is not null
            ? form[alternativeName].ToString()
            : value;
    }

    private static IFormFile? GetCoverFile(IFormFileCollection files) =>
        files["file"] ?? files["cover"] ?? files["cover_photo"] ?? files["image"];

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed record CreateItemInput(CreateItemRequest Request, IFormFile? File);

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
        return this.ToActionResult(await items.UploadImageAsync(objectId, new FileFormat(stream, file.Length, file.FileName, file.ContentType), cancellationToken));
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

    private static bool TryGetUserId(ClaimsPrincipal user, out int userId)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("id");
        return int.TryParse(value, out userId);
    }
}
