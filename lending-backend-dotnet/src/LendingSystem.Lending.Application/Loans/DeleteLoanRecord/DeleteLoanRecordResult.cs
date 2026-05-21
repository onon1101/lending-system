using System.Text.Json.Serialization;

namespace LendingSystem.Lending.Application.Loans;

public sealed record DeleteLoanRecordResult(
    [property: JsonPropertyName("deleted")] bool Deleted,
    [property: JsonPropertyName("message")] string Message);
