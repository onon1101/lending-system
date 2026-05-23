using Dapper;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using FluentValidation;
using LendingSystem.SharedKernel.Application.Abstractions;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

internal sealed class LoginCommandHandler(
    IPasswordHasher passwords,
    ITokenService tokens,
    IValidator<LoginCommand> validator,
    IQueryConnectionFactory queryConnectionFactory) : IRequestHandler<LoginCommand, Result<LoginResult>>
{
    public async Task<Result<LoginResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            var hasEmailError = validation.Errors.Any(x => x.PropertyName == nameof(LoginCommand.Email));
            return hasEmailError
                ? Result<LoginResult>.Failure(AuthErrors.InvalidEmail())
                : Result<LoginResult>.Failure(AuthErrors.InvalidCredentials());
        }

        var user = await GetUserInfo(request, cancellationToken);
        if (user is null 
            || string.IsNullOrWhiteSpace(user.PasswordHash) 
            || !passwords.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResult>.Failure(AuthErrors.InvalidCredentials());
        }

        var tokenPair = tokens.Generate(user.UserId, user.Username, user.Email, user.Role ?? "");
        return Result<LoginResult>.Success(new LoginResult(tokenPair.AccessToken, tokenPair.RefreshToken));
    }

    private async Task<UserRow?> GetUserInfo(LoginCommand request, CancellationToken cancellationToken)
    {
        const string sql = """
                            select
                                user_id as UserId,
                                name as Username,
                                email as Email,
                                password_hash as PasswordHash,
                                role as Role
                            from users
                            where email = @Email
                              and is_deleted = false;
                            """;

        var queryParams = new DynamicParameters();
        queryParams.Add("@Email", request.Email);
        
        var dapperParams = new CommandDefinition(sql, new { request.Email }, cancellationToken: cancellationToken);

        using var connection = queryConnectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserRow>(dapperParams);
    }

    private sealed record UserRow(
        long UserId,
        string Username,
        string Email,
        string? PasswordHash,
        string? Role);
}
