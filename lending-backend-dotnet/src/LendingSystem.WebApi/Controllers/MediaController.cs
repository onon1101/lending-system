using LendingSystem.Application.Media;
using LendingSystem.Application.Common;
using LendingSystem.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class MediaController(MediaService media) : ControllerBase
{
    [HttpPost("/api/v1/media/private")]
    public async Task<ActionResult<ApiResponse<MediaResponse>>> UploadPrivate(CancellationToken cancellationToken)
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

        var result = await media.UploadPrivateAsync(
            orderId,
            objectId,
            Request.Form["description"].ToString(),
            Request.Form["link"].ToString(),
            stream,
            file.Length,
            file.FileName,
            file.ContentType,
            cancellationToken);

        return this.ToCreatedActionResult("/api/v1/media/private", result);
    }
}
