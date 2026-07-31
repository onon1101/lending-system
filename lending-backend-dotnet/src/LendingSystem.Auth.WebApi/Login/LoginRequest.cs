using System.ComponentModel.DataAnnotations;

namespace LendingSystem.Auth.WebApi.Login;

/// <summary>
/// 登入請求
/// </summary>
/// <param name="Account">帳號</param>
/// <param name="Password">密碼</param>
public sealed record LoginRequest(
    [Required]
    string Account,
    
    [Required]
    string Password);