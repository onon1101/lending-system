using LendingSystem.Auth.Application.Auth.GetUserByName;
using LendingSystem.Auth.Application.Auth.GoogleLogin;
using LendingSystem.Auth.Application.Auth.Login;
using LendingSystem.Auth.Application.Auth.PasskeyRegistrationOption;
using LendingSystem.Auth.Application.Auth.RegisterUser;
using LendingSystem.Auth.Application.Auth.SearchUserByName;
using LendingSystem.WebApi.Configuration.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class AuthController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// 帳號密碼登入窗口
    /// </summary>
    /// <param name="command">登入請求</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>AccessToken 與 RefreshToken</returns>
    [HttpPost("/api/v1/auth/session")]
    [NoPermissionRequired]
    public async Task<ActionResult<ApiResponse<LoginResult>>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(command, cancellationToken));

    /// <summary>
    /// Google OAuth2 登入窗口
    /// </summary>
    /// <param name="command">Google OAuth2 登入請求</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>AccessToken、RefreshToken 與使用者資訊</returns>
    [HttpPost("/api/v1/auth/google")]
    [NoPermissionRequired]
    public async Task<ActionResult<ApiResponse<GoogleLoginResult>>> GoogleLogin(
        [FromBody] GoogleLoginCommand command,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(command, cancellationToken));

    /// <summary>
    /// 取得註冊 passkey 的註冊資訊
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("/api/v1/auth/passkey/registration-options")]
    public async Task<ActionResult<ApiResponse<PasskeyRegistrationOptionResult>>> PasskeyRegistrationOption(
        [FromBody] PasskeyRegistrationOptionQuery query,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(query, cancellationToken));

    /// <summary>
    /// 新使用者註冊端點
    /// </summary>
    /// <param name="command">註冊請求</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>新建立使用者的 UserId、Username 與 Email</returns>
    [HttpPost("/api/v1/users")]
    [NoPermissionRequired]
    public async Task<ActionResult<ApiResponse<RegisterUserResult>>> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var created = await mediator.Send(command, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/users/{created.Data!.Name}" : "", created);
    }

    /// <summary>
    /// 使用 Username 取得使用者資訊
    /// </summary>
    /// <param name="username"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("/api/v1/users/{username}")]
    [HasPermission(Permissions.ReadUsers)]
    public async Task<ActionResult<ApiResponse<GetUserByNameResult>>> GetUserByName(
        [FromRoute] string username,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new GetUserByNameQuery(username), cancellationToken));


    /// <summary>
    /// 使用 Username 與模糊搜索，取得使用者資訊
    /// </summary>
    /// <param name="username">使用者名稱</param>
    /// <param name="cancellationToken">取消作業的通知權杖</param>
    /// <returns>指定使用者的基本資訊</returns>
    [HttpGet("/api/v1/users/search/{username}")]
    [HasPermission(Permissions.ReadUsers)]
    public async Task<ActionResult<ApiResponse<SearchUserByNameResult>>> GetUserByBlurName(
        [FromRoute] string username,
        CancellationToken cancellationToken) =>
        this.ToActionResult(await mediator.Send(new SearchUserByNameQuery(Uri.UnescapeDataString(username)), cancellationToken));

}
