using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Auth.Application.Auth.Login;

public sealed record LoginCommand(
    [Required]
    [property: JsonPropertyName("email")] string Email,
    [Required]
    [property: JsonPropertyName("password")] string Password) : ICommand<LoginResult>;
