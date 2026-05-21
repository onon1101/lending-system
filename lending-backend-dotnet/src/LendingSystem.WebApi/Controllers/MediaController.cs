using LendingSystem.Lending.Application.Media;
using LendingSystem.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class MediaController(IMediator mediator) : ControllerBase
{
    [HttpPost("/api/v1/media/private")]
    public async Task<ActionResult<ApiResponse<UploadPrivateMediaResult>>> UploadPrivate(CancellationToken cancellationToken)
    {
        var file = Request.Form.Files["file"];
        if (file is null)
        {
            return this.ApiFailure<UploadPrivateMediaResult>(ControllerApiErrors.MissingFiles());
        }

        if (!int.TryParse(Request.Form["object_id"].ToString(), out var objectId))
        {
            return this.ApiFailure<UploadPrivateMediaResult>(ControllerApiErrors.MissingField("object_id"));
        }

        var orderIdValue = Request.Form["order_id"].ToString();
        int? orderId = null;
        if (!string.IsNullOrWhiteSpace(orderIdValue) && orderIdValue != "0")
        {
            if (!int.TryParse(orderIdValue, out var parsedOrderId))
            {
                return this.ApiFailure<UploadPrivateMediaResult>(ControllerApiErrors.MustBeInteger("order_id"));
            }

            orderId = parsedOrderId;
        }

        await using var stream = file.OpenReadStream();

        var result = await mediator.Send(new UploadPrivateMediaCommand(
            orderId,
            objectId,
            Request.Form["description"].ToString(),
            Request.Form["link"].ToString(),
            stream,
            file.Length,
            file.FileName,
            file.ContentType),
            cancellationToken);

        return this.ToCreatedActionResult("/api/v1/media/private", result);
    }
}
