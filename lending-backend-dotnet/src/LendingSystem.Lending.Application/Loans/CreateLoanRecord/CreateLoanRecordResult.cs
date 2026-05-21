using System.Text.Json.Serialization;

namespace LendingSystem.Lending.Application.Loans;

public sealed record CreateLoanRecordItemResult(
    [property: JsonPropertyName("object_detail_id")] int ObjectDetailId,
    [property: JsonPropertyName("object_id")] int ObjectId,
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("detail_status")] string DetailStatus,
    [property: JsonPropertyName("actual_return_date")] DateOnly? ActualReturnDate);

public sealed record CreateLoanRecordResult(
    [property: JsonPropertyName("order_id")] int OrderId,
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("start_date")] DateOnly OrderStartDate,
    [property: JsonPropertyName("end_date")] DateOnly OrderEndDate,
    [property: JsonPropertyName("order_status")] string OrderStatus,
    [property: JsonPropertyName("items")] IReadOnlyCollection<CreateLoanRecordItemResult> Items);
