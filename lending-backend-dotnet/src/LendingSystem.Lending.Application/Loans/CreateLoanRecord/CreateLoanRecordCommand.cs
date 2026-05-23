using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

public sealed record CreateLoanRecordCommand : ICommand<CreateLoanRecordResult>
{
    [JsonIgnore]
    public int UserId { get; init; }

    [JsonPropertyName("owner_username")]
    public string? OwnerUsername { get; init; }

    [JsonIgnore]
    public int? BorrowerId { get; init; }

    [JsonPropertyName("borrower_username")]
    public string? BorrowerUsername { get; init; }

    [JsonPropertyName("borrower_name")]
    public string? BorrowerName { get; init; }

    [JsonIgnore]
    public int ItemId { get; init; }

    [JsonPropertyName("object_name")]
    public string? ObjectName { get; init; }

    [JsonPropertyName("start_date")]
    public DateOnly StartDate { get; init; }

    [JsonPropertyName("end_date")]
    public DateOnly EndDate { get; init; }
}
