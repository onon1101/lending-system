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
                               name as {nameof(UserRow.Username)},
                               coalesce(email, '') as {nameof(UserRow.Email)} 
                           from users
                           where name ilike @Username
                             and is_deleted = false
                           order by user_id
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
