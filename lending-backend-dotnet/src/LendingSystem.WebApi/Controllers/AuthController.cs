using LendingSystem.Auth.Application.Auth;
using LendingSystem.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// 登入端點
    /// </summary>
    /// <param name="request">登入請求</param>
    /// <param name="cancellationToken"></param>
    /// <returns>AccessToken and RefreshToken</returns>
    [HttpPost("/api/v1/auth/session")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResult>>> Login([FromBody] LoginCommand command, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(command, cancellationToken));

    /// <summary>
    /// 使用 Google OAuth2，登入端點
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/auth/google")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<GoogleLoginResult>>> GoogleLogin(
        [FromBody] GoogleLoginCommand command,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(command, cancellationToken));

    /// <summary>
    /// 新使用者註冊端點
    /// </summary>
    /// <param name="request">註冊請求</param>
    /// <param name="cancellationToken"></param>
    /// <returns>UserId,Username,email</returns>
    [HttpPost("/api/v1/users")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<RegisterUserResult>>> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var created = await mediator.Send(command, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/users/{created.Data!.UserId}" : "", created);
    }

    /// <summary>
    /// 使用 UserId 取得使用者資訊
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/users/{userId:int}")]
    [Authorize(Roles="user,admin")]
    public async Task<ActionResult<ApiResponse<GetUserByIdResult>>> GetUserById([FromRoute] int userId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetUserByIdQuery(userId), cancellationToken));

    /// <summary>
    /// 使用 Username 取得使用者資訊
    /// </summary>
    /// <param name="username"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/users/search/{username}")]
    [Authorize(Roles="user,admin")]
    public async Task<ActionResult<ApiResponse<SearchUserByNameResult>>> GetUserByName([FromRoute] string username, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new SearchUserByNameQuery(Uri.UnescapeDataString(username)), cancellationToken));

    /// <summary>
    /// 軟刪除使用者
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("/api/v1/users/{userId:int}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<DeleteUserResult>>> DeleteByUserId([FromRoute] int userId, CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new DeleteUserCommand(userId), cancellationToken));
}
