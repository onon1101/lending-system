using LendingSystem.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace LendingSystem.SharedKernel.Infrastructure.Persistence.Tests;

public sealed class BorrowerDetailEntity_Test : InitializeData
{
    public override string TableName => "borrower_details";
    public override int Order => 3;

    public override async Task InsertBulkAsync(
        LendingDbContext db,
        CancellationToken cancellationToken = default)
    {
        const int borrowerCount = 10;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var userNames = Enumerable
            .Range(1, borrowerCount)
            .Select(id => $"testuser{id:00}")
            .ToArray();
        var users = await db.Users
            .Where(x => userNames.Contains(x.Name))
            .OrderBy(x => x.Name)
            .Select(x => new { x.UserId, x.Name })
            .ToArrayAsync(cancellationToken);

        var borrowerNames = Enumerable
            .Range(1, borrowerCount)
            .Select(id => $"Borrower test {id:00}")
            .ToArray();
        await db.BorrowerDetails
            .Where(x => borrowerNames.Contains(x.BorrowerName))
            .ExecuteDeleteAsync(cancellationToken);

        var borrowers = users
            .Select((user, index) => GenerateScript(index + 1, user.UserId, today))
            .ToArray();

        await db.BorrowerDetails.AddRangeAsync(borrowers, cancellationToken);
    }

    private static BorrowerDetailEntity GenerateScript(
        int id,
        long userId,
        DateOnly today) => new()
    {
        BorrowerDetailId = 2000 + id,
        UserId = userId,
        BorrowerName = $"Borrower test {id:00}",
        Link = $"https://example.com/borrowers/{id:00}",
        CreatedBy = "integration-test",
        CreatedAt = today,
        UpdatedBy = "integration-test",
        UpdatedAt = today
    };
}
