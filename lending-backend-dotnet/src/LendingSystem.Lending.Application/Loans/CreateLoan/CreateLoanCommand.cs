using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

public sealed record CreateLoanCommand : ICommand<CreateLoanResult>
{
    [JsonIgnore]
    public int UserId { get; init; }

    [JsonIgnore]
    public int? BorrowerId { get; init; }

    [JsonPropertyName("borrower_username")]
    public string? BorrowerUsername { get; init; }

    [JsonPropertyName("borrower_name")]
    public string? BorrowerName { get; init; }

    [JsonIgnore]
    public int[] ItemsId { get; init; } = [];

    [JsonPropertyName("items")]
    public BorrowingItemRequest[] Items { get; init; } = [];

    [JsonPropertyName("duration_days")]
    public int DurationDays { get; init; }
}

public sealed record BorrowingItemRequest(
    [property: JsonPropertyName("owner_username")] string OwnerUsername,
    [property: JsonPropertyName("object_name")] string ObjectName);
