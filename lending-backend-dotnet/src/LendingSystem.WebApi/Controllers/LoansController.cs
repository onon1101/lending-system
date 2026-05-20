using LendingSystem.Lending.Application.Loans;
using LendingSystem.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class LoansController(LoanService loans) : ControllerBase
{
    [HttpGet("/api/v1/users/{userId:int}/borrowings")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<UserLoanResponse>>>> GetUserActiveLoans([FromRoute] int userId, CancellationToken cancellationToken)
    {
        if (!CanAccessUser(User, userId))
        {
            return this.ApiFailure<IReadOnlyCollection<UserLoanResponse>>(ControllerApiErrors.AccessOwnBorrowingsOnly());
        }

        return this.ToActionResult(await loans.GetUserActiveLoansAsync(userId, cancellationToken));
    }

    [HttpPost("/api/v1/borrowings")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<UserLoanResponse>>> Create([FromBody] CreateLoanRequest request, CancellationToken cancellationToken)
    {
        var borrowerId = request.BorrowerId ?? request.UserId;
        if (!CanAccessUser(User, borrowerId))
        {
            return this.ApiFailure<UserLoanResponse>(ControllerApiErrors.CreateBorrowingsForSelfOnly());
        }

        var created = await loans.CreateAsync(request, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/borrowings/{created.Data!.OrderId}" : "", created);
    }

    [HttpPost("/api/v1/borrowings/{orderId:int}/items/{objectId:int}/return")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<UserLoanResponse>>> ReturnItem([FromRoute] int orderId, [FromRoute] int objectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await loans.ReturnItemAsync(orderId, objectId, cancellationToken));

    [HttpPost("/api/v1/management/borrowings")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<UserLoanResponse>>> CreateRecord([FromBody] CreateRecordRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanAccessUser(User, request.UserId))
        {
            return this.ApiFailure<UserLoanResponse>(ControllerApiErrors.ManageOwnItemRecordsOnly());
        }

        var created = await loans.CreateRecordAsync(request, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/borrowings/{created.Data!.OrderId}" : "", created);
    }

    [HttpDelete("/api/v1/management/borrowings/{orderId:int}")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<DeleteLoanRecordResponse>>> DeleteRecord(
        [FromRoute] int orderId,
        [FromQuery(Name = "user_id")] int userId,
        CancellationToken cancellationToken) =>
        !CanAccessUser(User, userId)
            ? this.ApiFailure<DeleteLoanRecordResponse>(ControllerApiErrors.ManageOwnItemRecordsOnly())
            : this.ToActionResult(await loans.DeleteRecordAsync(userId, orderId, cancellationToken));

    [HttpPatch("/api/v1/management/borrowings/{orderId:int}/time")]
    [Authorize(Roles = "user,admin")]
    public async Task<ActionResult<ApiResponse<UserLoanResponse>>> UpdateRecordTime(
        [FromRoute] int orderId,
        [FromBody] UpdateRecordTimeRequest request,
        CancellationToken cancellationToken) =>
        !CanAccessUser(User, request.UserId)
            ? this.ApiFailure<UserLoanResponse>(ControllerApiErrors.ManageOwnItemRecordsOnly())
            : this.ToActionResult(await loans.UpdateRecordTimeAsync(orderId, request, cancellationToken));

    [HttpGet("/api/v1/catalog/items/{objectId:int}/borrowings/history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<LoanRecordResponse>>>> GetItemHistory([FromRoute] int objectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await loans.GetHistoryByItemIdAsync(objectId, cancellationToken));

    private static bool CanAccessUser(ClaimsPrincipal user, int userId) =>
        user.IsInRole("admin") || TryGetUserId(user, out var currentUserId) && currentUserId == userId;

    private static bool TryGetUserId(ClaimsPrincipal user, out int userId)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("id");
        return int.TryParse(value, out userId);
    }
}
