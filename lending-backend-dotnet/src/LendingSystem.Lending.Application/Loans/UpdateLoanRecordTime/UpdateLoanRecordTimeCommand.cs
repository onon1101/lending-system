using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Lending.Application.Loans;

public sealed record UpdateLoanRecordTimeCommand : ICommand<UpdateLoanRecordTimeResult>
{
    [JsonIgnore]
    public int OrderId { get; init; }

    [JsonPropertyName("user_id")]
    public int UserId { get; init; }

    [JsonPropertyName("start_date")]
    public DateOnly? StartDate { get; init; }

    [JsonPropertyName("end_date")]
    public DateOnly? EndDate { get; init; }
}
