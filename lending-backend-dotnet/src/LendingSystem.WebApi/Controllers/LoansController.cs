using LendingSystem.Application.Loans;
using LendingSystem.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class LoansController(LoanService loans) : ControllerBase
{
    [HttpGet("/api/v1/users/{userId:int}/borrowings")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<UserLoanResponse>>>> GetUserActiveLoans([FromRoute] int userId, CancellationToken cancellationToken) =>
        this.ToActionResult(await loans.GetUserActiveLoansAsync(userId, cancellationToken));

    [HttpPost("/api/v1/borrowings")]
    public async Task<ActionResult<ApiResponse<UserLoanResponse>>> Create([FromBody] CreateLoanRequest request, CancellationToken cancellationToken)
    {
        var created = await loans.CreateAsync(request, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/borrowings/{created.Data!.OrderId}" : "", created);
    }

    [HttpPost("/api/v1/borrowings/{orderId:int}/items/{objectId:int}/return")]
    public async Task<ActionResult<ApiResponse<UserLoanResponse>>> ReturnItem([FromRoute] int orderId, [FromRoute] int objectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await loans.ReturnItemAsync(orderId, objectId, cancellationToken));

    [HttpPost("/api/v1/management/borrowings")]
    public async Task<ActionResult<ApiResponse<UserLoanResponse>>> CreateRecord([FromBody] CreateRecordRequest request,
        CancellationToken cancellationToken)
    {
        var created = await loans.CreateRecordAsync(request, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/borrowings/{created.Data!.OrderId}" : "", created);
    }

    [HttpDelete("/api/v1/management/borrowings/{orderId:int}")]
    public async Task<ActionResult<ApiResponse<DeleteLoanRecordResponse>>> DeleteRecord(
        [FromRoute] int orderId,
        [FromQuery(Name = "user_id")] int userId,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await loans.DeleteRecordAsync(userId, orderId, cancellationToken));

    [HttpPatch("/api/v1/management/borrowings/{orderId:int}/time")]
    public async Task<ActionResult<ApiResponse<UserLoanResponse>>> UpdateRecordTime(
        [FromRoute] int orderId,
        [FromBody] UpdateRecordTimeRequest request,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await loans.UpdateRecordTimeAsync(orderId, request, cancellationToken));

    [HttpGet("/api/v1/catalog/items/{objectId:int}/borrowings/history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<LoanRecordResponse>>>> GetItemHistory([FromRoute] int objectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await loans.GetHistoryByItemIdAsync(objectId, cancellationToken));
}
