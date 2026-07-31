using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.WebApi.Pagination;

namespace LendingSystem.Lending.Application.Items.GetItemsByUserName;

public sealed record GetItemsByUserNameQuery(string Username) : IQuery<PagedResult<GetItemsByUserNameResult>>;
