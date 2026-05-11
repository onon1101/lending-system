using LendingSystem.Application.Media;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class MediaController(MediaService media) : ControllerBase
{
    [HttpPost("/api/media/private")]
    [HttpPost("/api/v1/media/private")]
    public async Task<ActionResult<MediaResponse>> UploadPrivate(CancellationToken cancellationToken)
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

        return Created("/api/media/private", result);
    }
}
