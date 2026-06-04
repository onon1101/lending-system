using Dapper;
using FluentValidation;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth.Login;

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
                                u.user_id as UserId,
                                u.name as Username,
                                a.identifier as Email,
                                a.metadata_json ->> 'passwordHash' as PasswordHash,
                                u.role as Role
                            from users u
                            join user_auth_identities a on a.user_id = u.user_id
                            where a.type = 'LOCAL'
                              and a.identifier = @Email
                              and u.status = 'ACTIVE';
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
