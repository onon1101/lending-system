using System.Data;
using Dapper;
using LendingSystem.SharedKernel.WebApi.Pagination;

namespace LendingSystem.SharedKernel.Infrastructure.Pagination;

public static class DapperPagingExtensions
{
    public static async Task<PagedResult<T>> QueryPagedAsync<T>(
        this IDbConnection connection,
        string countSql,
        string dataSql,
        DynamicParameters parameters,
        PageRequest request,
        IDbTransaction? transaction = null,
        CancellationToken cancellationToken = default)
        where T : notnull
    {
            parameters.Add("Offset", request.Skip);
            parameters.Add("PageSize", request.PageSize);

            var sql = $"""
                       {countSql}
                       
                       {dataSql}
                       OFFSET @Offset ROWS
                       FETCH NEXT @PageSize ROWS ONLY;
                       """;

            await using var result = await connection.QueryMultipleAsync(
                new CommandDefinition(
                    commandText: sql,
                    parameters: parameters,
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            var totalItems = await result.ReadSingleAsync<int>();
            var items = (await result.ReadAsync<T>())
                .AsList();

            return new PagedResult<T>()
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalItems = totalItems
            };
    }
}