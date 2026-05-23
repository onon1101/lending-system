using Dapper;
using FluentValidation;
using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.Application.Common;
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
                           	name as {nameof(UserRow.Username)},
                           	email as {nameof(UserRow.Email)} 
                           FROM users
                           WHERE name = @Username
                             AND is_deleted = false;
                           """;

        var queryParams = new DynamicParameters();
        queryParams.Add("@Username", username);

        var dapperParams = new CommandDefinition(sql, queryParams, cancellationToken: cancellationToken);
        
        var connection = factory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<UserRow>(dapperParams); 
    }
    
    private sealed record UserRow(string Username, string Email);
}
