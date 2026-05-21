using System.Text.Json.Serialization;

namespace LendingSystem.Lending.Application.Loans;

public sealed record GetItemLoanHistoryResult(
    [property: JsonPropertyName("order_id")] int? OrderId,
    [property: JsonPropertyName("start_date")] DateOnly? StartDate,
    [property: JsonPropertyName("end_date")] DateOnly? EndDate,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("status")] string? Status);
