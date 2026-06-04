using Dapper;
using FluentValidation;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Domain.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth.GetUserByName;

internal sealed class GetUserByNameQueryHandler(
    IQueryConnectionFactory factory,
    IValidator<GetUserByNameQuery> validator) : IRequestHandler<GetUserByNameQuery, Result<GetUserByNameResult>>
{
    public async Task<Result<GetUserByNameResult>> Handle(GetUserByNameQuery request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<GetUserByNameResult>.Failure(AuthErrors.InvalidCredentials());
        }
        
        var user = await GetUserRowAsync(request.Username, cancellationToken);
        
        return user is null
            ? Result<GetUserByNameResult>.Failure(AuthErrors.UserNotFound())
            : Result<GetUserByNameResult>.Success(new GetUserByNameResult(user.Username, user.Email));
    }

    private async Task<UserRow?> GetUserRowAsync(string username, CancellationToken cancellationToken)
    {
        const string sql = $"""
                           SELECT
                           	u.name as {nameof(UserRow.Username)},
                           	coalesce(auth.email, '') as {nameof(UserRow.Email)}
                           FROM users u
                           LEFT JOIN LATERAL (
                               SELECT coalesce(a.metadata_json ->> 'email', a.identifier) as email
                               FROM user_auth_identities a
                               WHERE a.user_id = u.user_id
                               ORDER BY CASE WHEN a.type = 'LOCAL' THEN 0 ELSE 1 END, a.id
                               LIMIT 1
                           ) auth ON true
                           WHERE u.name = @Username
                             AND u.status = 'ACTIVE';
                           """;

        var queryParams = new DynamicParameters();
        queryParams.Add("@Username", username);

        var dapperParams = new CommandDefinition(sql, queryParams, cancellationToken: cancellationToken);
        
        var connection = factory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserRow>(dapperParams); 
    }
    
    private sealed record UserRow(string Username, string Email);
}
