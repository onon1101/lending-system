using System.Text.Json.Serialization;

namespace LendingSystem.Lending.Application.Loans.DeleteLoanRecord;

public sealed record DeleteLoanRecordResult(
    [property: JsonPropertyName("deleted")] bool Deleted,
    [property: JsonPropertyName("message")] string Message);
