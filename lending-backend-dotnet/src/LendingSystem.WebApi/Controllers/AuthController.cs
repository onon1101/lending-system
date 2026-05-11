using LendingSystem.Application.Auth;
using LendingSystem.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("/auth/login")]
    [HttpPost("/api/v1/auth/session")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken) =>
        this.ToActionResult(await auth.LoginAsync(request, cancellationToken));

    [HttpPost("/api/users")]
    [HttpPost("/api/v1/users")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> Register([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var created = await auth.RegisterAsync(request, cancellationToken);
        return this.ToCreatedActionResult(created.IsSuccess ? $"/api/v1/users/{created.Data!.UserId}" : "", created);
    }

    [HttpGet("/api/users/{userId:int}")]
    [HttpGet("/api/v1/users/{userId:int}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById([FromRoute] int userId, CancellationToken cancellationToken) =>
        this.ToActionResult(await auth.GetByIdAsync(userId, cancellationToken));

    [HttpGet("/api/users/name/{username}")]
    [HttpGet("/api/v1/users/search/{username}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserByName([FromRoute] string username, CancellationToken cancellationToken) =>
        this.ToActionResult(await auth.SearchByNameAsync(Uri.UnescapeDataString(username), cancellationToken));
}
