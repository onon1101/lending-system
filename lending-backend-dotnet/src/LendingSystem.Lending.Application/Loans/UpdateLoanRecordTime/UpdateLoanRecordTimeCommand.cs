using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

public sealed record UpdateLoanRecordTimeCommand : ICommand<UpdateLoanRecordTimeResult>
{
    [JsonIgnore]
    public long OrderId { get; init; }

    [JsonIgnore]
    public long UserId { get; init; }

    [JsonIgnore]
    public string BorrowingKey { get; init; } = "";

    [JsonPropertyName("owner_username")]
    public string? OwnerUsername { get; init; }

    [JsonPropertyName("start_date")]
    public DateOnly? StartDate { get; init; }

    [JsonPropertyName("end_date")]
    public DateOnly? EndDate { get; init; }
}
