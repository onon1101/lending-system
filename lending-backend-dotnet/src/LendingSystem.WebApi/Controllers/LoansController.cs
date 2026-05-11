using LendingSystem.Application.Loans;
using LendingSystem.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class LoansController(LoanService loans) : ControllerBase
{
    [HttpGet("/api/users/{userId:int}/loans")]
    [HttpGet("/api/v1/users/{userId:int}/borrowings")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<UserLoanResponse>>>> GetUserActiveLoans([FromRoute] int userId, CancellationToken cancellationToken) =>
        this.ToActionResult(await loans.GetUserActiveLoansAsync(userId, cancellationToken));

    [HttpPost("/api/loans")]
    [HttpPost("/api/v1/borrowings")]
    public async Task<ActionResult<ApiResponse<UserLoanResponse>>> Create([FromBody] CreateLoanRequest request, CancellationToken cancellationToken)
    {
        var created = await loans.CreateAsync(request, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/borrowings/{created.Data!.OrderId}" : "", created);
    }

    [HttpPost("/api/v1/borrowings/{orderId:int}/items/{objectId:int}/return")]
    public async Task<ActionResult<ApiResponse<UserLoanResponse>>> ReturnItem([FromRoute] int orderId, [FromRoute] int objectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await loans.ReturnItemAsync(orderId, objectId, cancellationToken));

    [HttpGet("/api/loans/items/history/{objectId:int}")]
    [HttpGet("/api/v1/catalog/items/{objectId:int}/borrowings/history")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<LoanRecordResponse>>>> GetItemHistory([FromRoute] int objectId, CancellationToken cancellationToken) =>
        this.ToActionResult(await loans.GetHistoryByItemIdAsync(objectId, cancellationToken));
}
