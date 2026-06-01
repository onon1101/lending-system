using System.Net;
using System.Net.Http.Json;
using LendingSystem.IntegrationTest.Framework.Infrastructure;
using LendingSystem.Lending.Application.Loans;
using LendingSystem.Lending.Application.Loans.CreateLoanRequest;
using LendingSystem.Lending.Application.Loans.GetLoanRequestByUser;
using LendingSystem.SharedKernel.Application.Common;
using LendingSystem.SharedKernel.Infrastructure.Persistence;
using Xunit;

namespace LendingSystem.IntegrationTest.Lending.Loans;

[WriteTest]
public sealed class LoansApiTests : IntegrationTestBase
{
    [Fact]
    public async Task GetUserActiveLoans_WithExistingLoan_ShouldReturnOk()
    {
        // Arrange
        await SeedUsersItemAndOrderAsync(status: "On Loan", borrowerId: 1000);
        using var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/users/owneruser/borrowings");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<IReadOnlyCollection<GetUserActiveLoansResult>>(response);
        Assert.True(result.Issuccess);
        Assert.Contains(result.Data!, loan => loan.OrderStatus == "On Loan");
    }

    [Fact]
    public async Task CreateLoanRequest_WithValidBody_ShouldReturnOk()
    {
        // Arrange
        await SeedUsersAndItemAsync(ownerId: 1001, ownerName: "otherowner");
        using var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/v1/users/borrowings/request",
            new CreateLoanRequestCommand
            {
                BorrowerName = "otherowner",
                ItemName = "testitem",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DurationDays = 7
            });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<CreateLoanRequestResult>(response);
        Assert.True(result.Issuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetLoanRequestsByCurrentUser_WithExistingRequest_ShouldReturnOk()
    {
        // Arrange
        await SeedUsersItemAndOrderAsync(status: "Requested", borrowerId: 1001);
        using var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/users/borrowings/request");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<IReadOnlyCollection<GetLoanRequestByUserResult>>(response);
        Assert.True(result.Issuccess);
        Assert.Contains(result.Data!, request => request.ItemName == "testitem");
    }

    [Fact]
    public async Task ReturnBorrowing_WithActiveLoan_ShouldReturnOk()
    {
        // Arrange
        var borrowingKey = await SeedUsersItemAndOrderAsync(status: "On Loan", borrowerId: 1000);
        using var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsync($"/api/v1/borrowings/{borrowingKey}/return", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<ReturnLoanItemResult>(response);
        Assert.True(result.Issuccess);
        Assert.Equal("Returned", result.Data!.OrderStatus);
    }

    [Fact]
    public async Task CreateLoanRecord_WithValidBody_ShouldReturnCreated()
    {
        // Arrange
        await SeedUsersAndItemAsync(ownerId: 1000, ownerName: "owneruser");
        using var client = Factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/v1/management/borrowings",
            new CreateLoanRecordCommand
            {
                OwnerUsername = "owneruser",
                BorrowerUsername = "borroweruser",
                ObjectName = "testitem",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-7),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow)
            });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await ParseJsonAsync<CreateLoanRecordResult>(response);
        Assert.True(result.Issuccess);
        Assert.Equal("Returned", result.Data!.OrderStatus);
    }

    [Fact]
    public async Task DeleteLoanRecord_WithExistingRecord_ShouldReturnOk()
    {
        // Arrange
        var borrowingKey = await SeedUsersItemAndOrderAsync(status: "Returned", borrowerId: 1001);
        using var client = Factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/v1/management/borrowings/{borrowingKey}?owner_username=owneruser");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<DeleteLoanRecordResult>(response);
        Assert.True(result.Issuccess);
        Assert.True(result.Data!.Deleted);
    }

    [Fact]
    public async Task UpdateLoanRecordTime_WithExistingRecord_ShouldReturnOk()
    {
        // Arrange
        var borrowingKey = await SeedUsersItemAndOrderAsync(status: "Returned", borrowerId: 1001);
        using var client = Factory.CreateClient();

        // Act
        var response = await client.PatchAsJsonAsync(
            $"/api/v1/management/borrowings/{borrowingKey}/time",
            new UpdateLoanRecordTimeCommand
            {
                OwnerUsername = "owneruser",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-10),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1)
            });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<UpdateLoanRecordTimeResult>(response);
        Assert.True(result.Issuccess);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1), result.Data!.OrderEndDate);
    }

    [Fact]
    public async Task GetItemLoanHistory_WithExistingRecord_ShouldReturnOk()
    {
        // Arrange
        await SeedUsersItemAndOrderAsync(status: "Returned", borrowerId: 1001);
        using var client = Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/v1/catalog/users/owneruser/items/testitem/borrowings/history");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await ParseJsonAsync<IReadOnlyCollection<GetItemLoanHistoryResult>>(response);
        Assert.True(result.Issuccess);
        Assert.Contains(result.Data!, history => history.Status == "Returned");
    }

    private async Task SeedUsersAndItemAsync(long ownerId, string ownerName)
    {
        await using var db = CreateDbContext();
        await db.Users.AddRangeAsync(
            new UserEntity
            {
                UserId = ownerId,
                Name = ownerName,
                Status = "ACTIVE",
                AuthIdentities =
                [
                    new UserAuthIdentityEntity
                    {
                        Id = 10000 + ownerId,
                        UserId = ownerId,
                        Type = "LOCAL",
                        Identifier = $"{ownerName}@example.com",
                        MetadataJson = $$"""{"email":"{{ownerName}}@example.com","passwordHash":"password"}"""
                    }
                ]
            },
            new UserEntity
            {
                UserId = ownerId == 1000 ? 1001 : 1000,
                Name = "borroweruser",
                Status = "ACTIVE",
                AuthIdentities =
                [
                    new UserAuthIdentityEntity
                    {
                        Id = 11000 + (ownerId == 1000 ? 1001 : 1000),
                        UserId = ownerId == 1000 ? 1001 : 1000,
                        Type = "LOCAL",
                        Identifier = "borrower@example.com",
                        MetadataJson = """{"email":"borrower@example.com","passwordHash":"password"}"""
                    }
                ]
            });
        await db.Items.AddAsync(new ItemEntity
        {
            ItemId = 2000,
            OwnerId = ownerId,
            ObjectName = "testitem",
            Maker = "maker",
            Material = "cotton",
            Description = "description",
            CurrentStatus = "Available"
        });
        await db.SaveChangesAsync();
    }

    private async Task<string> SeedUsersItemAndOrderAsync(string status, long borrowerId)
    {
        await SeedUsersAndItemAsync(ownerId: 1000, ownerName: "owneruser");
        await using var db = CreateDbContext();
        await db.BorrowerDetails.AddAsync(new BorrowerDetailEntity
        {
            BorrowerDetailId = 3000,
            UserId = borrowerId,
            BorrowerName = borrowerId == 1000 ? "owneruser" : "borroweruser",
            CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
        });
        await db.Orders.AddAsync(new OrderEntity
        {
            OrderId = 4000,
            BorrowerDetailId = 3000,
            ObjectId = 2000,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-7),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(7),
            ActualReturnDate = status == "Returned" ? DateOnly.FromDateTime(DateTime.UtcNow) : null,
            Status = status
        });
        await db.SaveChangesAsync();
        return PublicResourceKey.FromInt("borrowing", 4000);
    }
}
