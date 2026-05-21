using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

public sealed record CreateLoanRecordCommand : ICommand<CreateLoanRecordResult>
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
