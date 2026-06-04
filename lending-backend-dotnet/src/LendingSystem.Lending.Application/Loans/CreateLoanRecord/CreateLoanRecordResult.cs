using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;

namespace LendingSystem.Lending.Application.Loans.CreateLoanRecord;

public sealed record CreateLoanRecordItemResult(
    [property: JsonIgnore] long ObjectDetailId,
    [property: JsonIgnore] long ObjectId,
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("detail_status")] string DetailStatus,
    [property: JsonPropertyName("actual_return_date")] DateOnly? ActualReturnDate);

public sealed record CreateLoanRecordResult(
    [property: JsonIgnore] long OrderId,
    [property: JsonIgnore] long UserId,
    [property: JsonPropertyName("start_date")] DateOnly OrderStartDate,
    [property: JsonPropertyName("end_date")] DateOnly OrderEndDate,
    [property: JsonPropertyName("order_status")] string OrderStatus,
    [property: JsonPropertyName("items")] IReadOnlyCollection<CreateLoanRecordItemResult> Items)
{
    [JsonPropertyName("borrowing_key")]
    public string BorrowingKey => PublicResourceKey.FromInt("borrowing", OrderId);
}
