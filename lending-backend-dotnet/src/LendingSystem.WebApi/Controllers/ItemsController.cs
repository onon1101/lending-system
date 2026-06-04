using LendingSystem.Lending.Application.Items;
using LendingSystem.Lending.Application.Items.CreateItem;
using LendingSystem.Lending.Application.Items.GetAllItems;
using LendingSystem.Lending.Application.Items.GetItemByName;
using LendingSystem.Lending.Application.Items.GetItemMedia;
using LendingSystem.Lending.Application.Items.GetItemsByUserName;
using LendingSystem.Lending.Application.Items.UpdateItem;
using LendingSystem.Lending.Application.Items.UploadItemImage;
using LendingSystem.Lending.Application.Items.UploadItemMedia;
using LendingSystem.Lending.Application.Media;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.WebApi.Configuration.Authorization;
using LendingSystem.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class ItemsController(
    IMediator mediator,
    IExecutionContextAccessor executionContext) : ControllerBase
{
    /// <summary>
    /// 取得所有公開物品清單
    /// </summary>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>所有可瀏覽的物品摘要</returns>
    [HttpGet("/api/v1/catalog/items")]
    [NoPermissionRequired]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<GetAllItemsResult>>>> GetAll(CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetAllItemsQuery(), cancellationToken));

    /// <summary>
    /// 依照使用者名稱取得該使用者擁有的所有物品
    /// </summary>
    /// <param name="username">物品擁有者的使用者名稱</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>指定使用者擁有的物品摘要清單</returns>
    [HttpGet("/api/v1/catalog/items/user/{username}")]
    [NoPermissionRequired]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<GetItemsByUserNameResult>>>> GetItemsByUserName(
        [FromRoute] string username, CancellationToken cancellationToken)
        => this.ToActionResult(await mediator.Send(new GetItemsByUserNameQuery(username), cancellationToken));
        

    /// <summary>
    /// 建立目前登入使用者的新物品
    /// </summary>
    /// <param name="command">建立物品請求</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>新建立物品的資訊</returns>
    [HttpPost("/api/v1/catalog/items")]
    [HasPermission(Permissions.CreateItems)]
    public async Task<ActionResult<ApiResponse<CreateItemResult>>> Create(
        [FromBody] CreateItemCommand command,
        CancellationToken cancellationToken) =>
        await CreateForCurrentUserAsync(command, null, cancellationToken);

    /// <summary>
    /// 使用 multipart/form-data 建立目前登入使用者的新物品
    /// </summary>
    /// <param name="form">建立物品表單，包含物品資料與選填圖片</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>新建立物品的資訊</returns>
    [HttpPost("/api/v1/catalog/items/form")]
    [HasPermission(Permissions.CreateItems)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<CreateItemResult>>> CreateWithForm(
        [FromForm] CreateItemFormCommand form,
        CancellationToken cancellationToken)
    {
        var command = new CreateItemCommand(form.ObjectName, form.Maker, form.Material, form.Description);
        return await CreateForCurrentUserAsync(command, form.Image, cancellationToken);
    }

    private async Task<ActionResult<ApiResponse<CreateItemResult>>> CreateForCurrentUserAsync(
        CreateItemCommand command,
        IFormFile? image,
        CancellationToken cancellationToken)
    {
        var userId = executionContext.Current.User.UserId;
        if (userId <= 0)
        {
            return this.ApiFailure<CreateItemResult>(ControllerApiErrors.TokenInvalid());
        }

        await using var stream = image?.OpenReadStream();
        var fileFormat = image is null ? null : new FileFormat(stream!, image.Length, image.FileName, image.ContentType);
        var created = await mediator.Send(command with { UserId = userId, FileFormat = fileFormat }, cancellationToken);

        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/catalog/items" : "", created);
    }

    /// <summary>
    /// 使用擁有者 username 與物品名稱取得物品詳細資訊
    /// </summary>
    [HttpGet("/api/v1/catalog/users/{username}/items/{objectName}")]
    [NoPermissionRequired]
    public async Task<ActionResult<ApiResponse<GetItemByNameResult>>> GetByName(
        [FromRoute] string username,
        [FromRoute] string objectName,
        CancellationToken cancellation) =>
        this.ToActionResult(await mediator.Send(new GetItemByNameQuery(username, objectName), cancellation));

    /// <summary>
    /// 更新物品資訊
    /// </summary>
    /// <param name="username">物品擁有者 username</param>
    /// <param name="objectName">要更新的物品名稱</param>
    /// <param name="command">更新物品請求</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>更新後的物品資訊</returns>
    [HttpPut("/api/v1/catalog/users/{username}/items/{objectName}")]
    [HasPermission(Permissions.UpdateItems)]
    public async Task<ActionResult<ApiResponse<UpdateItemResult>>> Update([FromRoute] string username, [FromRoute] string objectName, [FromBody] UpdateItemCommand command, CancellationToken cancellationToken)
    {
        var userId = executionContext.Current.User.UserId;
        if (userId <= 0)
        {
            return this.ApiFailure<UpdateItemResult>(ControllerApiErrors.TokenInvalid());
        }

        return this.ToActionResult(await mediator.Send(command with
        {
            OwnerUsername = username,
            OriginalObjectName = objectName,
            CurrentUserId = userId,
            IsAdmin = executionContext.Current.User.IsAdmin
        }, cancellationToken));
    }

    /// <summary>
    /// 上傳或更新物品主圖
    /// </summary>
    /// <param name="username">物品擁有者 username</param>
    /// <param name="objectName">物品名稱</param>
    /// <param name="file">要上傳的圖片檔案</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>上傳後的物品圖片資訊</returns>
    [HttpPost("/api/v1/catalog/users/{username}/items/{objectName}/image")]
    [HasPermission(Permissions.UploadItemMedia)]
    public async Task<ActionResult<ApiResponse<UploadItemImageResult>>> UploadImage([FromRoute] string username, [FromRoute] string objectName, IFormFile file, CancellationToken cancellationToken)
    {
        var userId = executionContext.Current.User.UserId;
        if (userId <= 0)
        {
            return this.ApiFailure<UploadItemImageResult>(ControllerApiErrors.TokenInvalid());
        }

        await using var stream = file.OpenReadStream();
        return this.ToActionResult(
            await mediator.Send(
                new UploadItemImageCommand(
                    username,
                    objectName,
                    new FileFormat(
                        stream,
                        file.Length,
                        file.FileName,
                        file.ContentType),
                    userId,
                    executionContext.Current.User.IsAdmin),
                cancellationToken));
    }

    /// <summary>
    /// 上傳借閱相關媒體
    /// </summary>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>新增的借閱相關媒體資訊</returns>
    /// <remarks>
    /// 表單欄位包含 file、owner_username、object_name、borrowing_key、description、link。
    /// </remarks>
    [HttpPost("/api/v1/catalog/items/media")]
    [HasPermission(Permissions.UploadItemMedia)]
    public async Task<ActionResult<ApiResponse<UploadItemMediaResult>>> UploadMedia(CancellationToken cancellationToken)
    {
        var userId = executionContext.Current.User.UserId;
        if (userId <= 0)
        {
            return this.ApiFailure<UploadItemMediaResult>(ControllerApiErrors.TokenInvalid());
        }

        var file = Request.Form.Files["file"];
        if (file is null)
        {
            return this.ApiFailure<UploadItemMediaResult>(ControllerApiErrors.MissingFiles());
        }

        var ownerUsername = Request.Form["owner_username"].ToString();
        var objectName = Request.Form["object_name"].ToString();
        var borrowingKey = Request.Form["borrowing_key"].ToString();
        if (string.IsNullOrWhiteSpace(ownerUsername) || string.IsNullOrWhiteSpace(objectName))
        {
            return this.ApiFailure<UploadItemMediaResult>(ControllerApiErrors.MissingField(missingField: "owner_username/object_name"));
        }

        await using var stream = file.OpenReadStream();

        var result = await mediator.Send(new UploadItemMediaCommand(
            borrowingKey,
            ownerUsername,
            objectName,
            Request.Form["description"].ToString(),
            Request.Form["link"].ToString(),
            stream,
            file.Length,
            file.FileName,
            file.ContentType,
            userId,
            executionContext.Current.User.IsAdmin),
            cancellationToken);

        return this.ToCreatedActionResult($"/api/v1/catalog/users/{Uri.EscapeDataString(ownerUsername)}/items/{Uri.EscapeDataString(objectName)}/media", result);
    }

    /// <summary>
    /// 取得物品相關媒體
    /// </summary>
    /// <param name="username">物品擁有者 username</param>
    /// <param name="objectName">物品名稱</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>物品展示媒體與借閱相關媒體清單</returns>
    [HttpGet("/api/v1/catalog/users/{username}/items/{objectName}/media")]
    [NoPermissionRequired]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<GetItemMediaResult>>>> GetMedia([FromRoute] string username, [FromRoute] string objectName, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetItemMediaQuery(username, objectName), cancellationToken));

}

public sealed class CreateItemFormCommand
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
