using System.Text.Json.Serialization;

namespace LendingSystem.Application.Loans;

public sealed record CreateLoanRequest(
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("items_id")] int[] ItemsId,
    [property: JsonPropertyName("duration_hours")] int DurationHours);

public sealed record ReturnLoanItemRequest(
    [property: JsonPropertyName("object_id")] int ObjectId);

public sealed record LoanItemDetailResponse(
    [property: JsonPropertyName("object_detail_id")] int ObjectDetailId,
    [property: JsonPropertyName("object_id")] int ObjectId,
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("detail_status")] string DetailStatus,
    [property: JsonPropertyName("actual_return_time")] DateTimeOffset? ActualReturnTime);

public sealed record UserLoanResponse(
    [property: JsonPropertyName("order_id")] int OrderId,
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("start_time")] DateTimeOffset OrderStartTime,
    [property: JsonPropertyName("end_time")] DateTimeOffset OrderEndTime,
    [property: JsonPropertyName("order_status")] string OrderStatus,
    [property: JsonPropertyName("items")] IReadOnlyCollection<LoanItemDetailResponse> Items);

public sealed record LoanRecordResponse(
    [property: JsonPropertyName("start_time")] DateTimeOffset? StartTime,
    [property: JsonPropertyName("end_time")] DateTimeOffset? EndTime,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("status")] string? Status);
