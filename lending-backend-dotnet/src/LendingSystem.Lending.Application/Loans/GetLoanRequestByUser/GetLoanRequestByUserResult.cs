using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Lending.Application.Loans.GetLoanRequestByUser;

public sealed record GetLoanRequestByUserResult(
    [property: JsonIgnore] long OrderId,
    [property: JsonPropertyName("item_name")] string ItemName,
    [property: JsonPropertyName("borrower_name")] string BorrowerName,
    [property: JsonPropertyName("borrower_username")] string BorrowerUsername,
    [property: JsonPropertyName("start_date")] DateOnly StartDate,
    [property: JsonPropertyName("end_date")] DateOnly EndDate,
    [property: JsonPropertyName("status")] string Status)
{
    [JsonPropertyName("borrowing_key")]
    public string BorrowingKey => PublicResourceKey.FromInt("borrowing", OrderId);
}
