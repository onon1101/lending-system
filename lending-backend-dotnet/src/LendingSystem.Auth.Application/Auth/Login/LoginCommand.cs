using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

public sealed record LoginCommand(
    [Required]
    [property: JsonPropertyName("email")] string Email,
    [Required]
    [property: JsonPropertyName("password")] string Password) : ICommand<LoginResult>;
