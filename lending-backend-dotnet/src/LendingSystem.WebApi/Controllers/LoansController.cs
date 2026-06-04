using LendingSystem.Lending.Application.Loans;
using LendingSystem.Lending.Application.Loans.CreateLoanRecord;
using LendingSystem.Lending.Application.Loans.CreateLoanRequest;
using LendingSystem.Lending.Application.Loans.DeleteLoanRecord;
using LendingSystem.Lending.Application.Loans.GetItemLoanHistory;
using LendingSystem.Lending.Application.Loans.GetLoanRequestByUser;
using LendingSystem.Lending.Application.Loans.GetUserActiveLoans;
using LendingSystem.Lending.Application.Loans.ReturnLoanItem;
using LendingSystem.Lending.Application.Loans.UpdateLoanRecordTime;
using LendingSystem.WebApi.Configuration.Authorization;
using LendingSystem.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class LoansController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// 取得使用者目前借閱中的紀錄
    /// </summary>
    /// <param name="username">借閱者 username</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>使用者目前尚未歸還的借閱紀錄</returns>
    [HttpGet("/api/v1/users/{username}/borrowings")]
    [HasPermission(Permissions.ReadBorrowings)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<GetUserActiveLoansResult>>>> GetUserActiveLoans([FromRoute] string username, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetUserActiveLoansQuery(username), cancellationToken));

    /// <summary>
    /// 建立借閱請求
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/users/borrowings/request")]
    [HasPermission(Permissions.CreateBorrowings)]
    public async Task<ActionResult<ApiResponse<CreateLoanRequestResult>>> Create(
        [FromBody] CreateLoanRequestCommand command, CancellationToken cancellationToken)
    {
        var created = await mediator.Send(command, cancellationToken);
        // return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/borrowings/{created.Data!.BorrowingKey}" : "", created);
        return this.ToActionResult(created);
    }

    /// <summary>
    /// 取得目前使用者收到的借閱請求
    /// </summary>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>目前使用者名下物品的待審核借閱請求</returns>
    [HttpGet("/api/v1/users/borrowings/request")]
    [HasPermission(Permissions.ReadBorrowings)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<GetLoanRequestByUserResult>>>> GetLoanRequestsByCurrentUser(CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetLoanRequestByUserQuery(), cancellationToken));

    // /// <summary>
    // /// 建立借閱單
    // /// </summary>
    // /// <param name="command">建立借閱請求</param>
    // /// <param name="cancellationToken">取消作業的通知權杖</param>
    // /// <returns>新建立的借閱單資訊</returns>
    // [HttpPost("/api/v1/borrowings")]
    // [HasPermission(Permissions.CreateBorrowings)]
    // public async Task<ActionResult<ApiResponse<CreateLoanResult>>> Create([FromBody] CreateLoanCommand command, CancellationToken cancellationToken)
    // {
    //     var created = await mediator.Send(command, cancellationToken);
    //     return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/borrowings/{created.Data!.BorrowingKey}" : "", created);
    // }

    /// <summary>
    /// 歸還借閱物品
    /// </summary>
    /// <param name="borrowingKey">借閱公開 key</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>歸還後的借閱單資訊</returns>
    [HttpPost("/api/v1/borrowings/{borrowingKey}/return")]
    [HasPermission(Permissions.ReturnBorrowings)]
    public async Task<ActionResult<ApiResponse<ReturnLoanItemResult>>> ReturnItem([FromRoute] string borrowingKey, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new ReturnLoanItemCommand(borrowingKey), cancellationToken));

    /// <summary>
    /// 建立物品擁有者管理用借閱紀錄
    /// </summary>
    /// <param name="command">建立管理用借閱紀錄請求</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>新建立的借閱紀錄資訊</returns>
    [HttpPost("/api/v1/management/borrowings")]
    [HasPermission(Permissions.ManageBorrowings)]
    public async Task<ActionResult<ApiResponse<CreateLoanRecordResult>>> CreateRecord([FromBody] CreateLoanRecordCommand command,
        CancellationToken cancellationToken)
    {
        var created = await mediator.Send(command, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/borrowings/{created.Data!.BorrowingKey}" : "", created);
    }

    /// <summary>
    /// 刪除物品擁有者管理用借閱紀錄
    /// </summary>
    /// <param name="borrowingKey">借閱公開 key</param>
    /// <param name="ownerUsername">物品擁有者 username</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>刪除結果</returns>
    [HttpDelete("/api/v1/management/borrowings/{borrowingKey}")]
    [HasPermission(Permissions.ManageBorrowings)]
    public async Task<ActionResult<ApiResponse<DeleteLoanRecordResult>>> DeleteRecord(
        [FromRoute] string borrowingKey,
        [FromQuery(Name = "owner_username")] string ownerUsername,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteLoanRecordCommand(ownerUsername, borrowingKey), cancellationToken));

    /// <summary>
    /// 更新物品擁有者管理用借閱紀錄時間
    /// </summary>
    /// <param name="borrowingKey">借閱公開 key</param>
    /// <param name="command">更新借閱時間請求</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>更新後的借閱紀錄資訊</returns>
    [HttpPatch("/api/v1/management/borrowings/{borrowingKey}/time")]
    [HasPermission(Permissions.ManageBorrowings)]
    public async Task<ActionResult<ApiResponse<UpdateLoanRecordTimeResult>>> UpdateRecordTime(
        [FromRoute] string borrowingKey,
        [FromBody] UpdateLoanRecordTimeCommand command,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(command with { BorrowingKey = borrowingKey }, cancellationToken));

    /// <summary>
    /// 取得物品借閱歷史
    /// </summary>
    /// <param name="username">物品擁有者 username</param>
    /// <param name="objectName">物品名稱</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>指定物品的借閱歷史紀錄</returns>
    [HttpGet("/api/v1/catalog/users/{username}/items/{objectName}/borrowings/history")]
    [NoPermissionRequired]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<GetItemLoanHistoryResult>>>> GetItemHistory([FromRoute] string username, [FromRoute] string objectName, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetItemLoanHistoryQuery(username, objectName), cancellationToken));
}
