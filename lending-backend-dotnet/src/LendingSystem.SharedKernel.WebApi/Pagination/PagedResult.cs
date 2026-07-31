namespace LendingSystem.SharedKernel.WebApi.Pagination;

public sealed class PagedResult<T>
    where T : notnull
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    
    public int TotalPages =>
        TotalItems == 0
            ? 0
            : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}