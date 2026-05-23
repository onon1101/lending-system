using LendingSystem.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Tests;

public sealed class OrderEntity_Test : InitializeData
{
    public override string TableName => "orders";
    public override int Order => 4;

    public override async Task InsertBulkAsync(
        LendingDbContext db,
        CancellationToken cancellationToken = default)
    {
        const int orderCount = 10;
        var borrowerNames = Enumerable
            .Range(1, orderCount)
            .Select(id => $"Borrower test {id:00}")
            .ToArray();
        var itemNames = Enumerable
            .Range(1, orderCount)
            .Select(id => $"Item test {id:00}")
            .ToArray();
        var borrowers = await db.BorrowerDetails
            .Where(x => borrowerNames.Contains(x.BorrowerName))
            .OrderBy(x => x.BorrowerName)
            .Select(x => new { x.BorrowerDetailId, x.BorrowerName })
            .ToArrayAsync(cancellationToken);
        var items = await db.Items
            .Where(x => itemNames.Contains(x.ObjectName))
            .OrderBy(x => x.ObjectName)
            .Select(x => new { x.ItemId, x.ObjectName })
            .ToArrayAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var orders = borrowers
            .Zip(items)
            .Select((pair, index) => GenerateScript(
                index + 1,
                pair.First.BorrowerDetailId,
                pair.Second.ItemId,
                today))
            .ToArray();

        await db.Orders.AddRangeAsync(orders, cancellationToken);
    }

    private static OrderEntity GenerateScript(
        int id,
        long borrowerDetailId,
        long itemId,
        DateOnly today)
    {
        var startDate = today.AddDays(-id);
        var endDate = today.AddDays(id);
        var isReturned = id % 3 == 0;

        return new OrderEntity
        {
            OrderId = 3000 + id,
            BorrowerDetailId = borrowerDetailId,
            ObjectId = itemId,
            StartDate = startDate,
            EndDate = endDate,
            ActualReturnDate = isReturned ? today : null,
            Status = isReturned ? "Returned" : "On Loan"
        };
    }
}
