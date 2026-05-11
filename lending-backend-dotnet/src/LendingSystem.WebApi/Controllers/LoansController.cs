using LendingSystem.Application.Loans;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class LoansController(LoanService loans) : ControllerBase
{
    [HttpGet("/api/users/{userId:int}/loans")]
    public async Task<ActionResult<IReadOnlyCollection<UserLoanResponse>>> GetUserActiveLoans([FromRoute] int userId, CancellationToken cancellationToken) =>
        Ok(await loans.GetUserActiveLoansAsync(userId, cancellationToken));

    [HttpPost("/api/loans")]
    public async Task<ActionResult<UserLoanResponse>> Create([FromBody] CreateLoanRequest request, CancellationToken cancellationToken)
    {
        var created = await loans.CreateAsync(request, cancellationToken);
        return Created($"/api/loans/{created.OrderId}", created);
    }

    [HttpGet("/api/loans/items/history/{objectId:int}")]
    public async Task<ActionResult<IReadOnlyCollection<LoanRecordResponse>>> GetItemHistory([FromRoute] int objectId, CancellationToken cancellationToken) =>
        Ok(await loans.GetHistoryByItemIdAsync(objectId, cancellationToken));
}
