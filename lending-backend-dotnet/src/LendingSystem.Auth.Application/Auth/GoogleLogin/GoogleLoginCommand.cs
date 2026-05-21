using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

public sealed record GoogleLoginCommand(
    [Required]
    [property: JsonPropertyName("id_token")] string IdToken) : ICommand<GoogleLoginResult>;
