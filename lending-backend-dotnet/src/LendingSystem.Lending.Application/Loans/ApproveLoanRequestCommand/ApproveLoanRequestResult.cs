using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Lending.Application.Loans.ApproveLoanRequestCommand;

public sealed record ApproveLoanRequestResult(
    [property: JsonIgnore] long OrderId,
    [property: JsonPropertyName("order_status")] string OrderStatus)
{
    [JsonPropertyName("borrowing_key")]
    public string BorrowingKey => PublicResourceKey.FromInt("borrowing", OrderId);
}
