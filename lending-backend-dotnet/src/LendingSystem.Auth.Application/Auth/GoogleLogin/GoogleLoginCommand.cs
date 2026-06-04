using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Auth.Application.Auth.GoogleLogin;

public sealed record GoogleLoginCommand(
    [Required]
    [property: JsonPropertyName("id_token")] string IdToken) : ICommand<GoogleLoginResult>;
