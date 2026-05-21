using LendingSystem.Lending.Application.Loans;
using LendingSystem.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class LoansController(IMediator mediator) : ControllerBase
{
    [HttpGet("/api/v1/users/{userId:int}/borrowings")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<GetUserActiveLoansResult>>>> GetUserActiveLoans([FromRoute] int userId, CancellationToken cancellationToken)
    {
        if (!CanAccessUser(User, userId))
        {
            return this.ApiFailure<IReadOnlyCollection<GetUserActiveLoansResult>>(ControllerApiErrors.AccessOwnBorrowingsOnly());
        }

        return this.ToActionResult(await mediator.Send(new GetUserActiveLoansQuery(userId), cancellationToken));
    }

    [HttpPost("/api/v1/borrowings")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<CreateLoanResult>>> Create([FromBody] CreateLoanCommand command, CancellationToken cancellationToken)
    {
        var borrowerId = command.BorrowerId ?? command.UserId;
        if (!CanAccessUser(User, borrowerId))
        {
            return this.ApiFailure<CreateLoanResult>(ControllerApiErrors.CreateBorrowingsForSelfOnly());
        }

        var created = await mediator.Send(command, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/borrowings/{created.Data!.OrderId}" : "", created);
    }

    [HttpPost("/api/v1/borrowings/{orderId:int}/items/{objectId:int}/return")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<ReturnLoanItemResult>>> ReturnItem([FromRoute] int orderId, [FromRoute] int objectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new ReturnLoanItemCommand(orderId, objectId), cancellationToken));

    [HttpPost("/api/v1/management/borrowings")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<CreateLoanRecordResult>>> CreateRecord([FromBody] CreateLoanRecordCommand command,
        CancellationToken cancellationToken)
    {
        if (!CanAccessUser(User, command.UserId))
        {
            return this.ApiFailure<CreateLoanRecordResult>(ControllerApiErrors.ManageOwnItemRecordsOnly());
        }

        var created = await mediator.Send(command, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/borrowings/{created.Data!.OrderId}" : "", created);
    }

    [HttpDelete("/api/v1/management/borrowings/{orderId:int}")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<DeleteLoanRecordResult>>> DeleteRecord(
        [FromRoute] int orderId,
        [FromQuery(Name = "user_id")] int userId,
        CancellationToken cancellationToken) =>
        !CanAccessUser(User, userId)
            ? this.ApiFailure<DeleteLoanRecordResult>(ControllerApiErrors.ManageOwnItemRecordsOnly())
            : this.ToActionResult(await mediator.Send(new DeleteLoanRecordCommand(userId, orderId), cancellationToken));

    [HttpPatch("/api/v1/management/borrowings/{orderId:int}/time")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<UpdateLoanRecordTimeResult>>> UpdateRecordTime(
        [FromRoute] int orderId,
        [FromBody] UpdateLoanRecordTimeCommand command,
        CancellationToken cancellationToken) =>
        !CanAccessUser(User, command.UserId)
            ? this.ApiFailure<UpdateLoanRecordTimeResult>(ControllerApiErrors.ManageOwnItemRecordsOnly())
            : this.ToActionResult(await mediator.Send(command with { OrderId = orderId }, cancellationToken));

    [HttpGet("/api/v1/catalog/items/{objectId:int}/borrowings/history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<GetItemLoanHistoryResult>>>> GetItemHistory([FromRoute] int objectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetItemLoanHistoryQuery(objectId), cancellationToken));

    private static bool CanAccessUser(ClaimsPrincipal user, int userId) =>
        user.IsInRole("admin") || TryGetUserId(user, out var currentUserId) && currentUserId == userId;

    private static bool TryGetUserId(ClaimsPrincipal user, out int userId)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("id");
        return int.TryParse(value, out userId);
    }
}
