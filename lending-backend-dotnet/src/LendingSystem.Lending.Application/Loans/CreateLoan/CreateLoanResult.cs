using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Lending.Application.Loans;

public sealed record CreateLoanItemResult(
    [property: JsonIgnore] int ObjectDetailId,
    [property: JsonIgnore] int ObjectId,
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("detail_status")] string DetailStatus,
    [property: JsonPropertyName("actual_return_date")] DateOnly? ActualReturnDate);

public sealed record CreateLoanResult(
    [property: JsonIgnore] int OrderId,
    [property: JsonIgnore] int UserId,
    [property: JsonPropertyName("start_date")] DateOnly OrderStartDate,
    [property: JsonPropertyName("end_date")] DateOnly OrderEndDate,
    [property: JsonPropertyName("order_status")] string OrderStatus,
    [property: JsonPropertyName("items")] IReadOnlyCollection<CreateLoanItemResult> Items)
{
    [JsonPropertyName("borrowing_key")]
    public string BorrowingKey => PublicResourceKey.FromInt("borrowing", OrderId);
}
