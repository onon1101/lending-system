using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

public sealed record RegisterUserCommand(
    [Required]
    [property: JsonPropertyName("name")] string Name,
    [Required]
    [property: JsonPropertyName("email")] string Email,
    [Required]
    [property: JsonPropertyName("password_hash")] string PasswordHash) : ICommand<RegisterUserResult>;
