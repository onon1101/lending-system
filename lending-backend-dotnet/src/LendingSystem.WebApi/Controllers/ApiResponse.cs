using System.Text.Json.Serialization;

namespace LendingSystem.WebApi.Controllers;

public sealed record ApiResponse<T>
{
    [JsonPropertyName("Data")]
    public T? Data { get; init; }

    [JsonPropertyName("Issuccess")]
    public bool Issuccess { get; init; }

    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; init; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; init; }

    [JsonConstructor]
    public ApiResponse(T? data, bool issuccess, string? errorCode, string? errorMessage)
    {
        Data = data;
        Issuccess = issuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static ApiResponse<T> Success(T data) => new(data, true, null, null);

    public static ApiResponse<T> Failure(string errorCode, string errorMessage) =>
        new(default, false, errorCode, errorMessage);
}
