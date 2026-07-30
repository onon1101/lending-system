using LendingSystem.Lending.Application.Media;
using LendingSystem.Lending.Application.Media.UploadPrivateMedia;
using LendingSystem.WebApi.Configuration.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class MediaController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// 上傳借閱相關的私人媒體
    /// </summary>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>新增的借閱媒體資訊</returns>
    /// <remarks>
    /// 表單欄位包含 file、owner_username、object_name、borrowing_key、description、link。
    /// </remarks>
    [HttpPost("/api/v1/media/private")]
    [HasPermission(Permissions.UploadItemMedia)]
    public async Task<ActionResult<ApiResponse<UploadPrivateMediaResult>>> UploadPrivate(CancellationToken cancellationToken)
    {
        var file = Request.Form.Files["file"];
        if (file is null)
        {
            return this.ApiFailure<UploadPrivateMediaResult>(ControllerApiErrors.MissingFiles());
        }

        var ownerUsername = Request.Form["owner_username"].ToString();
        var objectName = Request.Form["object_name"].ToString();
        var borrowingKey = Request.Form["borrowing_key"].ToString();
        if (string.IsNullOrWhiteSpace(ownerUsername) || string.IsNullOrWhiteSpace(objectName))
        {
            return this.ApiFailure<UploadPrivateMediaResult>(ControllerApiErrors.MissingField("owner_username/object_name"));
        }

        await using var stream = file.OpenReadStream();

        var result = await mediator.Send(new UploadPrivateMediaCommand(
            borrowingKey,
            ownerUsername,
            objectName,
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
