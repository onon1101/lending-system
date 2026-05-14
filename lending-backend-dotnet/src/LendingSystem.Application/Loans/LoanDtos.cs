using System.Text.Json.Serialization;

namespace LendingSystem.Application.Loans;

public sealed class CreateLoanRequest
{
    [JsonPropertyName("user_id")]
    public int UserId { get; init; }

    [JsonPropertyName("borrower_id")]
    public int? BorrowerId { get; init; }

    [JsonPropertyName("borrower_name")]
    public string? BorrowerName { get; init; }

    [JsonPropertyName("items_id")]
    public int[] ItemsId { get; init; } = [];

    [JsonPropertyName("duration_days")]
    public int DurationDays { get; init; }
}

public sealed class CreateRecordRequest
{
    [JsonPropertyName("user_id")]
    public int UserId { get; init; }

    [JsonPropertyName("borrower_id")]
    public int? BorrowerId { get; init; }

    [JsonPropertyName("borrower_name")]
    public string? BorrowerName { get; init; }

    [JsonPropertyName("item_id")]
    public int ItemId { get; init; }

    [JsonPropertyName("start_date")]
    public DateOnly StartDate { get; init; }

    [JsonPropertyName("end_date")]
    public DateOnly EndDate { get; init; }
}

public sealed class UpdateRecordTimeRequest
{
    [JsonPropertyName("user_id")]
    public int UserId { get; init; }

    [JsonPropertyName("start_date")]
    public DateOnly? StartDate { get; init; }

    [JsonPropertyName("end_date")]
    public DateOnly? EndDate { get; init; }
}

public sealed record ReturnLoanItemRequest(
    [property: JsonPropertyName("object_id")] int ObjectId);

public sealed record LoanItemDetailResponse(
    [property: JsonPropertyName("object_detail_id")] int ObjectDetailId,
    [property: JsonPropertyName("object_id")] int ObjectId,
    [property: JsonPropertyName("object_name")] string ObjectName,
    [property: JsonPropertyName("detail_status")] string DetailStatus,
    [property: JsonPropertyName("actual_return_date")] DateOnly? ActualReturnDate);

public sealed record UserLoanResponse(
    [property: JsonPropertyName("order_id")] int OrderId,
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("start_date")] DateOnly OrderStartDate,
    [property: JsonPropertyName("end_date")] DateOnly OrderEndDate,
    [property: JsonPropertyName("order_status")] string OrderStatus,
    [property: JsonPropertyName("items")] IReadOnlyCollection<LoanItemDetailResponse> Items);

public sealed record DeleteLoanRecordResponse(
    [property: JsonPropertyName("deleted")] bool Deleted,
    [property: JsonPropertyName("message")] string Message);

public sealed record LoanRecordResponse(
    [property: JsonPropertyName("order_id")] int? OrderId,
    [property: JsonPropertyName("start_date")] DateOnly? StartDate,
    [property: JsonPropertyName("end_date")] DateOnly? EndDate,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("status")] string? Status);
