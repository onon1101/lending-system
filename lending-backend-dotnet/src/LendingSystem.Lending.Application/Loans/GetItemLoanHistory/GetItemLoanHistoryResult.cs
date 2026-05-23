using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Lending.Application.Loans;

public sealed record GetItemLoanHistoryResult(
    [property: JsonIgnore] long? OrderId,
    [property: JsonPropertyName("start_date")] DateOnly? StartDate,
    [property: JsonPropertyName("end_date")] DateOnly? EndDate,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("status")] string? Status)
{
    [JsonPropertyName("borrowing_key")]
    public string? BorrowingKey => OrderId is null ? null : PublicResourceKey.FromInt("borrowing", OrderId.Value);
}
