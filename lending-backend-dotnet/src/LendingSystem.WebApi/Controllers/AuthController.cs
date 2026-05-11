using LendingSystem.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSystem.WebApi.Controllers;

[ApiController]
public sealed class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("/auth/login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken) =>
        Ok(await auth.LoginAsync(request, cancellationToken));

    [HttpPost("/api/users")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<UserResponse>> Register([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var created = await auth.RegisterAsync(request, cancellationToken);
        return Created($"/api/users/{created.UserId}", created);
    }

    [HttpGet("/api/users/{userId:int}")]
    public async Task<ActionResult<UserResponse>> GetUserById([FromRoute] int userId, CancellationToken cancellationToken) =>
        Ok(await auth.GetByIdAsync(userId, cancellationToken));

    [HttpGet("/api/users/name/{username}")]
    public async Task<ActionResult<UserResponse>> GetUserByName([FromRoute] string username, CancellationToken cancellationToken) =>
        Ok(await auth.SearchByNameAsync(Uri.UnescapeDataString(username), cancellationToken));
}
