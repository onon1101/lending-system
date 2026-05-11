using LendingSystem.Application.Items;
using LendingSystem.Application.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class ItemsController(ItemService items) : ControllerBase
{
    [HttpGet("/api/items")]
    [HttpGet("/api/v1/catalog/items")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<ItemSummaryResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await items.GetAllAsync(cancellationToken));

    [HttpPost("/api/items")]
    [HttpPost("/api/v1/catalog/items")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ItemResponse>> Create([FromBody] CreateItemRequest request, CancellationToken cancellationToken)
    {
        var created = await items.CreateAsync(request, cancellationToken);
        return Created($"/api/v1/catalog/items/{created.ObjectId}", created);
    }

    [HttpGet("/api/items/{objectId:int}")]
    [HttpGet("/api/v1/catalog/items/{objectId:int}")]
    public async Task<ActionResult<ItemResponse>> GetById([FromRoute] int objectId, CancellationToken cancellationToken) =>
        Ok(await items.GetByIdAsync(objectId, cancellationToken));

    [HttpPut("/api/items/{objectId:int}")]
    [HttpPut("/api/v1/catalog/items/{objectId:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ItemResponse>> Update([FromRoute] int objectId, [FromBody] UpdateItemRequest request, CancellationToken cancellationToken) =>
        Ok(await items.UpdateAsync(objectId, request, cancellationToken));

    [HttpPost("/api/items/{objectId:int}/image")]
    [HttpPost("/api/v1/catalog/items/{objectId:int}/image")]
    public async Task<ActionResult<ItemResponse>> UploadImage([FromRoute] int objectId, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await items.UploadImageAsync(objectId, stream, file.Length, file.FileName, file.ContentType, cancellationToken));
    }

    [HttpPost("/api/items/media")]
    [HttpPost("/api/v1/catalog/items/media")]
    public async Task<ActionResult<MediaResponse>> UploadMedia(CancellationToken cancellationToken)
    {
        var file = Request.Form.Files["file"];
        if (file is null)
        {
            return BadRequest(new { error = "Missing File" });
        }

        var objectId = int.Parse(Request.Form["object_id"].ToString());
        var orderIdValue = Request.Form["order_id"].ToString();
        int? orderId = string.IsNullOrWhiteSpace(orderIdValue) || orderIdValue == "0" ? null : int.Parse(orderIdValue);
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

        return Created($"/api/v1/catalog/items/{objectId}/media", result);
    }

    [HttpGet("/api/items/media/{objectId:int}")]
    [HttpGet("/api/v1/catalog/items/{objectId:int}/media")]
    public async Task<ActionResult<IReadOnlyCollection<ItemMediaResponse>>> GetMedia([FromRoute] int objectId, CancellationToken cancellationToken) =>
        Ok(await items.GetMediaAsync(objectId, cancellationToken));
}
