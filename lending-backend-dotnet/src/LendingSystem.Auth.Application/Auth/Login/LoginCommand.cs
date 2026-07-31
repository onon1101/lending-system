using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LendingSystem.SharedKernel.Application.Abstractions;

namespace LendingSystem.Auth.Application.Auth.Login;

/// <summary>
/// 登入 Command
/// </summary>
/// <param name="Account"></param>
/// <param name="Password"></param>
public sealed record LoginCommand(
    Account Account,
    Password Password) : ICommand<LoginResult>;
