namespace LendingSystem.SharedKernel.WebApi.Pagination;

public sealed class PageRequest
{
    private const int MaxPageSize = 100;
    
    public int PageNumber { get; }
    public int PageSize { get; }

    public int Skip => (PageNumber - 1) * PageSize;

    public PageRequest(int pageNumber = 1, int pageSize = 20)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageSize),
                $"PageSize 必須介於 1 到 {MaxPageSize} 之間");
        }

        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}