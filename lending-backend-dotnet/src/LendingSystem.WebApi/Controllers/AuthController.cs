using LendingSystem.Application.Auth;
using LendingSystem.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    /// <summary>
    /// 登入端點
    /// </summary>
    /// <param name="request">登入請求</param>
    /// <param name="cancellationToken"></param>
    /// <returns>AccessToken and RefreshToken</returns>
    [HttpPost("/api/v1/auth/session")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await auth.LoginAsync(request, cancellationToken));

    /// <summary>
    /// 新使用者註冊端點
    /// </summary>
    /// <param name="request">註冊請求</param>
    /// <param name="cancellationToken"></param>
    /// <returns>UserId,Username,email</returns>
    [HttpPost("/api/v1/users")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Register([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var created = await auth.RegisterAsync(request, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/users/{created.Data!.UserId}" : "", created);
    }

    [HttpGet("/api/v1/users/{userId:int}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById([FromRoute] int userId, CancellationToken cancellationToken) =>
        this.ToActionResult(await auth.GetByIdAsync(userId, cancellationToken));

    [HttpGet("/api/v1/users/search/{username}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserByName([FromRoute] string username, CancellationToken cancellationToken) =>
        this.ToActionResult(await auth.SearchByNameAsync(Uri.UnescapeDataString(username), cancellationToken));

    [HttpDelete("/api/v1/users/{userId:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<DeleteResponse>>> DeleteByUserId([FromRoute] int userId, CancellationToken cancellationToken) =>
        this.ToActionResult(await auth.DeleteByIdAsync(userId, cancellationToken));
}
