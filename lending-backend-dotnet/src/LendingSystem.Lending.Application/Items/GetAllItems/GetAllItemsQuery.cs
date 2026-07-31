using LendingSystem.SharedKernel.Application.Abstractions;
using LendingSystem.SharedKernel.WebApi.Pagination;

namespace LendingSystem.Lending.Application.Items.GetAllItems;

public sealed record GetAllItemsQuery : IQuery<PagedResult<GetAllItemsResult>>;
