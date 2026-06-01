using Dapper;
using LendingSystem.Auth.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
using MediatR;

namespace LendingSystem.Auth.Application.Auth;

internal sealed class SearchUserByNameQueryHandler(
    IQueryConnectionFactory factory) : IRequestHandler<SearchUserByNameQuery, Result<SearchUserByNameResult>>
{
    public async Task<Result<SearchUserByNameResult>> Handle(SearchUserByNameQuery request, CancellationToken cancellationToken)
    {
        var user = await SearchUserByNameAsync(request.Username, cancellationToken);
        
        return user is null
            ? Result<SearchUserByNameResult>.Failure(AuthErrors.UserNotFound())
            : Result<SearchUserByNameResult>.Success(new SearchUserByNameResult(user.Username, user.Email));
    }

    private async Task<UserRow?> SearchUserByNameAsync(string username,
        CancellationToken cancellationToken)
    {
        const string sql = $"""
                           select
                               u.name as {nameof(UserRow.Username)},
                               coalesce(auth.email, '') as {nameof(UserRow.Email)}
                           from users u
                           left join lateral (
                               select coalesce(a.metadata_json ->> 'email', a.identifier) as email
                               from user_auth_identities a
                               where a.user_id = u.user_id
                               order by case when a.type = 'LOCAL' then 0 else 1 end, a.id
                               limit 1
                           ) auth on true
                           where u.name ilike @Username
                             and u.status = 'ACTIVE'
                           order by u.user_id
                           limit 1;
                           """;

        var queryParam = new DynamicParameters();
        queryParam.Add("@Username", $"{username}%");

        var connection = factory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<UserRow>(new CommandDefinition(
            sql, queryParam, cancellationToken: cancellationToken));
    }

    private sealed record UserRow(string Username, string Email);
}
