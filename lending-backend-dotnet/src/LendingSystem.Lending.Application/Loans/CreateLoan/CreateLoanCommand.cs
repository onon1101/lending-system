using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

public sealed record CreateLoanCommand : ICommand<CreateLoanResult>
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
